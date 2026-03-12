using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionSyndicale.Infrastructure.Data;
using GestionSyndicale.Core.Entities;
using GestionSyndicale.Core.DTOs.Revenue;
using System.Security.Claims;

namespace GestionSyndicale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class RevenuesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RevenuesController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public RevenuesController(
        ApplicationDbContext context,
        ILogger<RevenuesController> logger,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _environment = environment;
    }

    /// <summary>
    /// Matrice des cotisations mensuelles par immeuble
    /// </summary>
    [HttpGet("contributions/matrix")]
    public async Task<ActionResult<ContributionsMatrixDto>> GetContributionsMatrix([FromQuery] int year = 0)
    {
        try
        {
            if (year == 0)
            {
                year = DateTime.Now.Year;
            }

            var monthlyContributionAmount = _configuration.GetValue<decimal>("Finance:MonthlyContributionAmount", 100);

            // Récupérer tous les immeubles
            var buildings = await _context.Buildings
                .OrderBy(b => b.BuildingNumber)
                .Select(b => new
                {
                    b.Id,
                    Code = b.BuildingNumber
                })
                .ToListAsync();

            var buildingsData = new List<BuildingMonthlyContributionsDto>();
            var monthlyTotals = Enumerable.Repeat(0m, 12).ToList();

            foreach (var building in buildings)
            {
                var monthlyAmounts = new List<decimal>();

                // Pour chaque mois (1 à 12)
                for (int month = 1; month <= 12; month++)
                {
                    // Compter le nombre de paiements effectués pour ce mois par les appartements de cet immeuble
                    var paymentsCount = await _context.MonthlyPayments
                        .Include(mp => mp.Apartment)
                        .Where(mp => mp.Apartment.BuildingId == building.Id
                                  && mp.Year == year
                                  && mp.Month == month
                                  && mp.IsPaid == true)
                        .CountAsync();

                    var amount = paymentsCount * monthlyContributionAmount;
                    monthlyAmounts.Add(amount);
                    monthlyTotals[month - 1] += amount;
                }

                buildingsData.Add(new BuildingMonthlyContributionsDto
                {
                    BuildingId = building.Id,
                    BuildingCode = building.Code,
                    MonthlyAmounts = monthlyAmounts,
                    RowTotal = monthlyAmounts.Sum()
                });
            }

            var result = new ContributionsMatrixDto
            {
                Year = year,
                Buildings = buildingsData,
                MonthlyTotals = monthlyTotals,
                YearTotal = monthlyTotals.Sum(),
                MonthlyContributionAmount = monthlyContributionAmount
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contributions matrix");
            return StatusCode(500, new { message = "Error getting contributions matrix" });
        }
    }

    /// <summary>
    /// Liste paginée des autres revenus
    /// </summary>
    [HttpGet("other")]
    public async Task<ActionResult<OtherRevenuePagedResultDto>> GetOtherRevenues(
        [FromQuery] int year = 0,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        try
        {
            if (year == 0)
            {
                year = DateTime.Now.Year;
            }

            var query = _context.OtherRevenues
                .Include(or => or.RecordedBy)
                .Where(or => or.RevenueDate.Year == year)
                .AsQueryable();

            // Filtre recherche
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(or =>
                    or.Title.Contains(search) ||
                    (or.Description != null && or.Description.Contains(search)));
            }

            // Total pour l'année
            var totalAmount = await query.SumAsync(or => or.Amount);
            var totalCount = await query.CountAsync();

            // Pagination
            var revenues = await query
                .OrderByDescending(or => or.RevenueDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Charger tous les compteurs d'attachments en une seule requête
            var revenueIds = revenues.Select(r => r.Id).ToList();
            var attachmentCounts = await _context.Documents
                .Where(d => d.Category == "OtherRevenue" && revenueIds.Contains(d.RelatedEntityId!.Value))
                .GroupBy(d => d.RelatedEntityId)
                .Select(g => new { RevenueId = g.Key!.Value, Count = g.Count() })
                .ToDictionaryAsync(x => x.RevenueId, x => x.Count);

            var items = revenues.Select(revenue => new OtherRevenueListDto
            {
                Id = revenue.Id,
                RevenueDate = revenue.RevenueDate,
                Title = revenue.Title,
                Amount = revenue.Amount,
                Description = revenue.Description,
                AttachmentsCount = attachmentCounts.ContainsKey(revenue.Id) ? attachmentCounts[revenue.Id] : 0,
                RecordedAt = revenue.RecordedAt,
                RecordedByName = revenue.RecordedBy.FirstName + " " + revenue.RecordedBy.LastName
            }).ToList();

            return Ok(new OtherRevenuePagedResultDto
            {
                Items = items,
                TotalCount = totalCount,
                TotalAmount = totalAmount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting other revenues");
            return StatusCode(500, new { message = "Error getting other revenues" });
        }
    }

    /// <summary>
    /// Détails d'un autre revenu
    /// </summary>
    [HttpGet("other/{id}")]
    public async Task<ActionResult<OtherRevenueDetailDto>> GetOtherRevenue(int id)
    {
        try
        {
            var revenue = await _context.OtherRevenues
                .Include(or => or.RecordedBy)
                .FirstOrDefaultAsync(or => or.Id == id);

            if (revenue == null)
            {
                return NotFound(new { message = "Revenu non trouvé" });
            }

            // Charger les documents liés
            var attachments = await _context.Documents
                .Include(d => d.UploadedBy)
                .Where(d => d.Category == "OtherRevenue" && d.RelatedEntityId == id)
                .ToListAsync();

            var dto = new OtherRevenueDetailDto
            {
                Id = revenue.Id,
                RevenueDate = revenue.RevenueDate,
                Title = revenue.Title,
                Amount = revenue.Amount,
                Description = revenue.Description,
                RecordedAt = revenue.RecordedAt,
                RecordedByUserId = revenue.RecordedByUserId,
                RecordedByName = revenue.RecordedBy.FirstName + " " + revenue.RecordedBy.LastName,
                Attachments = attachments.Select(d => new DocumentDto
                {
                    Id = d.Id,
                    FileName = d.FileName,
                    FileType = d.FileType,
                    FileSize = d.FileSize,
                    UploadedAt = d.UploadedAt,
                    UploadedByName = d.UploadedBy.FirstName + " " + d.UploadedBy.LastName
                }).ToList()
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting other revenue {RevenueId}", id);
            return StatusCode(500, new { message = "Error getting revenue" });
        }
    }

    /// <summary>
    /// Créer un autre revenu
    /// </summary>
    [HttpPost("other")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<OtherRevenueDetailDto>> CreateOtherRevenue([FromBody] CreateOtherRevenueDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var revenue = new OtherRevenue
            {
                RevenueDate = dto.RevenueDate,
                Title = dto.Title,
                Amount = dto.Amount,
                Description = dto.Description,
                RecordedByUserId = userId,
                RecordedAt = DateTime.UtcNow
            };

            _context.OtherRevenues.Add(revenue);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOtherRevenue), new { id = revenue.Id }, await GetRevenueDto(revenue.Id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating other revenue");
            return StatusCode(500, new { message = "Error creating revenue" });
        }
    }

    /// <summary>
    /// Modifier un autre revenu
    /// </summary>
    [HttpPut("other/{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<OtherRevenueDetailDto>> UpdateOtherRevenue(int id, [FromBody] UpdateOtherRevenueDto dto)
    {
        try
        {
            var revenue = await _context.OtherRevenues.FindAsync(id);

            if (revenue == null)
            {
                return NotFound(new { message = "Revenu non trouvé" });
            }

            revenue.RevenueDate = dto.RevenueDate;
            revenue.Title = dto.Title;
            revenue.Amount = dto.Amount;
            revenue.Description = dto.Description;

            await _context.SaveChangesAsync();

            return Ok(await GetRevenueDto(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating other revenue {RevenueId}", id);
            return StatusCode(500, new { message = "Error updating revenue" });
        }
    }

    /// <summary>
    /// Supprimer un autre revenu (Soft Delete)
    /// </summary>
    [HttpDelete("other/{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteOtherRevenue(int id)
    {
        try
        {
            var revenue = await _context.OtherRevenues.FindAsync(id);

            if (revenue == null)
            {
                return NotFound(new { message = "Revenu non trouvé" });
            }

            revenue.IsDeleted = true;
            revenue.DeletedAt = DateTime.UtcNow;

            // Soft delete des documents associés
            var documents = await _context.Documents
                .Where(d => d.Category == "OtherRevenue" && d.RelatedEntityId == id)
                .ToListAsync();

            foreach (var doc in documents)
            {
                doc.IsDeleted = true;
                doc.DeletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Revenu supprimé avec succès" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting other revenue {RevenueId}", id);
            return StatusCode(500, new { message = "Error deleting revenue" });
        }
    }

    /// <summary>
    /// Ajouter des pièces jointes à un revenu
    /// </summary>
    [HttpPost("other/{id}/attachments")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UploadAttachments(int id, [FromForm] List<IFormFile> files)
    {
        try
        {
            var revenue = await _context.OtherRevenues.FindAsync(id);
            if (revenue == null)
            {
                return NotFound(new { message = "Revenu non trouvé" });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            if (files == null || !files.Any())
            {
                return BadRequest(new { message = "Aucun fichier fourni" });
            }

            var maxFileSize = _configuration.GetValue<long>("FileStorage:MaxFileSizeMB", 10) * 1024 * 1024;
            var allowedTypes = new[] { "image/jpeg", "image/png", "application/pdf" };

            var uploadPath = Path.Combine(_environment.ContentRootPath, "uploads", "revenues");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var uploadedDocs = new List<Document>();

            foreach (var file in files)
            {
                if (file.Length > maxFileSize)
                {
                    return BadRequest(new { message = $"Le fichier {file.FileName} dépasse la taille maximale autorisée" });
                }

                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    return BadRequest(new { message = $"Le type de fichier {file.FileName} n'est pas autorisé" });
                }

                var uniqueFileName = $"revenue{id}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var document = new Document
                {
                    Title = file.FileName,
                    FileName = file.FileName,
                    FilePath = filePath,
                    FileType = file.ContentType,
                    FileSize = file.Length,
                    Category = "OtherRevenue",
                    RelatedEntityId = id,
                    UploadedByUserId = userId,
                    UploadedAt = DateTime.UtcNow,
                    IsPublic = false
                };

                _context.Documents.Add(document);
                uploadedDocs.Add(document);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = $"{files.Count} fichier(s) téléchargé(s) avec succès", uploadedCount = files.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading attachments for revenue {RevenueId}", id);
            return StatusCode(500, new { message = "Error uploading attachments" });
        }
    }

    /// <summary>
    /// Télécharger une pièce jointe
    /// </summary>
    [HttpGet("other/{revenueId}/attachments/{documentId}/download")]
    public async Task<IActionResult> DownloadAttachment(int revenueId, int documentId)
    {
        try
        {
            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == documentId && d.RelatedEntityId == revenueId && d.Category == "OtherRevenue");

            if (document == null)
            {
                return NotFound(new { message = "Document non trouvé" });
            }

            if (!System.IO.File.Exists(document.FilePath))
            {
                return NotFound(new { message = "Fichier physique non trouvé" });
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(document.FilePath);
            return File(fileBytes, document.FileType, document.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading attachment {DocumentId}", documentId);
            return StatusCode(500, new { message = "Error downloading attachment" });
        }
    }

    /// <summary>
    /// Supprimer une pièce jointe (Soft Delete)
    /// </summary>
    [HttpDelete("other/{revenueId}/attachments/{documentId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteAttachment(int revenueId, int documentId)
    {
        try
        {
            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == documentId && d.RelatedEntityId == revenueId && d.Category == "OtherRevenue");

            if (document == null)
            {
                return NotFound(new { message = "Document non trouvé" });
            }

            document.IsDeleted = true;
            document.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Document supprimé avec succès" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting attachment {DocumentId}", documentId);
            return StatusCode(500, new { message = "Error deleting attachment" });
        }
    }

    /// <summary>
    /// Totaux globaux pour le bandeau
    /// </summary>
    [HttpGet("totals")]
    public async Task<ActionResult<RevenuesTotalDto>> GetRevenuesTotals([FromQuery] int year = 0)
    {
        try
        {
            if (year == 0)
            {
                year = DateTime.Now.Year;
            }

            // Total cotisations
            var monthlyContributionAmount = _configuration.GetValue<decimal>("Finance:MonthlyContributionAmount", 100);
            var contributionsCount = await _context.MonthlyPayments
                .Where(mp => mp.Year == year && mp.IsPaid)
                .CountAsync();
            var contributionsTotal = contributionsCount * monthlyContributionAmount;

            // Total autres revenus
            var otherRevenuesTotal = await _context.OtherRevenues
                .Where(or => or.RevenueDate.Year == year)
                .SumAsync(or => or.Amount);

            return Ok(new RevenuesTotalDto
            {
                Year = year,
                ContributionsTotal = contributionsTotal,
                OtherRevenuesTotal = otherRevenuesTotal,
                GrandTotal = contributionsTotal + otherRevenuesTotal
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenues totals");
            return StatusCode(500, new { message = "Error getting totals" });
        }
    }

    // Helper methods
    private async Task<OtherRevenueDetailDto> GetRevenueDto(int id)
    {
        var revenue = await _context.OtherRevenues
            .Include(or => or.RecordedBy)
            .FirstAsync(or => or.Id == id);

        // Charger les documents liés
        var attachments = await _context.Documents
            .Include(d => d.UploadedBy)
            .Where(d => d.Category == "OtherRevenue" && d.RelatedEntityId == id)
            .ToListAsync();

        return new OtherRevenueDetailDto
        {
            Id = revenue.Id,
            RevenueDate = revenue.RevenueDate,
            Title = revenue.Title,
            Amount = revenue.Amount,
            Description = revenue.Description,
            RecordedAt = revenue.RecordedAt,
            RecordedByUserId = revenue.RecordedByUserId,
            RecordedByName = revenue.RecordedBy.FirstName + " " + revenue.RecordedBy.LastName,
            Attachments = attachments.Select(d => new DocumentDto
            {
                Id = d.Id,
                FileName = d.FileName,
                FileType = d.FileType,
                FileSize = d.FileSize,
                UploadedAt = d.UploadedAt,
                UploadedByName = d.UploadedBy.FirstName + " " + d.UploadedBy.LastName
            }).ToList()
        };
    }
}
