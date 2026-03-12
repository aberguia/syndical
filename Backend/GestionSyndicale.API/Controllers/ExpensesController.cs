using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionSyndicale.Infrastructure.Data;
using GestionSyndicale.Core.Entities;
using GestionSyndicale.Core.DTOs.Expense;
using System.Security.Claims;
using System.Globalization;

namespace GestionSyndicale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class ExpensesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ExpensesController> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExpensesController(
        ApplicationDbContext context,
        ILogger<ExpensesController> logger,
        IWebHostEnvironment environment)
    {
        _context = context;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Liste des dépenses avec filtres et pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ExpensePagedResultDto>> GetExpenses(
        [FromQuery] int year = 0,
        [FromQuery] int? month = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        try
        {
            // Année par défaut = année courante
            if (year == 0)
            {
                year = DateTime.Now.Year;
            }

            var query = _context.Expenses
                .Include(e => e.Supplier)
                .Include(e => e.RecordedBy)
                .Include(e => e.Attachments)
                .Where(e => e.ExpenseDate.Year == year)
                .AsQueryable();

            // Filtre mois
            if (month.HasValue && month.Value >= 1 && month.Value <= 12)
            {
                query = query.Where(e => e.ExpenseDate.Month == month.Value);
            }

            // Note: Category filtering is no longer supported as categories are now accessed through Supplier.ServiceCategory

            // Recherche
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(e =>
                    e.Description.ToLower().Contains(search) ||
                    (e.InvoiceNumber != null && e.InvoiceNumber.ToLower().Contains(search)) ||
                    (e.Supplier != null && e.Supplier.Name.ToLower().Contains(search)));
            }

            // Total count et amount avant pagination
            var totalCount = await query.CountAsync();
            var totalAmount = await query.SumAsync(e => e.Amount);

            // Pagination
            var expenses = await query
                .OrderByDescending(e => e.ExpenseDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new ExpenseListDto
                {
                    Id = e.Id,
                    ExpenseDate = e.ExpenseDate,
                    CategoryId = 0,
                    CategoryName = e.Supplier != null ? e.Supplier.ServiceCategory : null,
                    Amount = e.Amount,
                    Description = e.Description,
                    InvoiceNumber = e.InvoiceNumber,
                    SupplierName = e.Supplier != null ? e.Supplier.Name : null,
                    AttachmentsCount = e.Attachments.Count,
                    RecordedAt = e.RecordedAt,
                    RecordedByName = e.RecordedBy.FirstName + " " + e.RecordedBy.LastName
                })
                .ToListAsync();

            return Ok(new ExpensePagedResultDto
            {
                Items = expenses,
                TotalCount = totalCount,
                TotalAmount = totalAmount,
                Page = page,
                PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expenses");
            return StatusCode(500, new { message = "Error retrieving expenses" });
        }
    }

    /// <summary>
    /// Détails d'une dépense
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ExpenseDetailDto>> GetExpense(int id)
    {
        try
        {
            var expense = await _context.Expenses
                .Include(e => e.Supplier)
                .Include(e => e.RecordedBy)
                .Include(e => e.Attachments)
                    .ThenInclude(a => a.UploadedBy)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (expense == null)
            {
                return NotFound(new { message = "Dépense non trouvée" });
            }

            var dto = new ExpenseDetailDto
            {
                Id = expense.Id,
                ExpenseDate = expense.ExpenseDate,
                CategoryId = 0,
                CategoryName = expense.Supplier?.ServiceCategory,
                SupplierId = expense.SupplierId,
                SupplierName = expense.Supplier?.Name,
                Amount = expense.Amount,
                Description = expense.Description,
                InvoiceNumber = expense.InvoiceNumber,
                Notes = expense.Notes,
                RecordedAt = expense.RecordedAt,
                RecordedByName = expense.RecordedBy.FirstName + " " + expense.RecordedBy.LastName,
                Attachments = expense.Attachments.Select(a => new AttachmentDto
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    FileType = a.FileType,
                    FileSize = a.FileSize,
                    UploadedAt = a.UploadedAt,
                    UploadedByName = a.UploadedBy.FirstName + " " + a.UploadedBy.LastName
                }).ToList()
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expense {ExpenseId}", id);
            return StatusCode(500, new { message = "Error retrieving expense" });
        }
    }

    /// <summary>
    /// Créer une nouvelle dépense
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ExpenseDetailDto>> CreateExpense([FromBody] CreateExpenseDto dto)
    {
        try
        {
            // Validation
            if (dto.Amount <= 0)
            {
                return BadRequest(new { message = "Le montant doit être supérieur à 0" });
            }

            if (string.IsNullOrWhiteSpace(dto.Description))
            {
                return BadRequest(new { message = "La description est obligatoire" });
            }

            // Vérifier le fournisseur si spécifié
            if (dto.SupplierId.HasValue)
            {
                var supplierExists = await _context.Suppliers
                    .AnyAsync(s => s.Id == dto.SupplierId.Value);
                
                if (!supplierExists)
                {
                    return BadRequest(new { message = "Fournisseur invalide" });
                }
            }

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var expense = new Expense
            {
                SupplierId = dto.SupplierId,
                Description = dto.Description,
                Amount = dto.Amount,
                ExpenseDate = dto.ExpenseDate,
                InvoiceNumber = dto.InvoiceNumber,
                Notes = dto.Notes,
                RecordedByUserId = currentUserId,
                RecordedAt = DateTime.UtcNow
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetExpense), new { id = expense.Id }, await GetExpenseDto(expense.Id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expense");
            return StatusCode(500, new { message = "Error creating expense" });
        }
    }

    /// <summary>
    /// Modifier une dépense
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ExpenseDetailDto>> UpdateExpense(int id, [FromBody] UpdateExpenseDto dto)
    {
        try
        {
            var expense = await _context.Expenses.FindAsync(id);
            
            if (expense == null)
            {
                return NotFound(new { message = "Dépense non trouvée" });
            }

            // Validation
            if (dto.Amount <= 0)
            {
                return BadRequest(new { message = "Le montant doit être supérieur à 0" });
            }

            if (string.IsNullOrWhiteSpace(dto.Description))
            {
                return BadRequest(new { message = "La description est obligatoire" });
            }

            // Vérifier le fournisseur si spécifié
            if (dto.SupplierId.HasValue)
            {
                var supplierExists = await _context.Suppliers
                    .AnyAsync(s => s.Id == dto.SupplierId.Value);
                
                if (!supplierExists)
                {
                    return BadRequest(new { message = "Fournisseur invalide" });
                }
            }

            expense.SupplierId = dto.SupplierId;
            expense.Description = dto.Description;
            expense.Amount = dto.Amount;
            expense.ExpenseDate = dto.ExpenseDate;
            expense.InvoiceNumber = dto.InvoiceNumber;
            expense.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            return Ok(await GetExpenseDto(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating expense {ExpenseId}", id);
            return StatusCode(500, new { message = "Error updating expense" });
        }
    }

    /// <summary>
    /// Supprimer une dépense (Soft Delete - SuperAdmin uniquement)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        try
        {
            var expense = await _context.Expenses
                .Include(e => e.Attachments)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (expense == null)
            {
                return NotFound(new { message = "Dépense non trouvée" });
            }

            // Soft delete de la dépense
            expense.IsDeleted = true;
            expense.DeletedAt = DateTime.UtcNow;

            // Soft delete des pièces jointes associées (pour la traçabilité)
            foreach (var attachment in expense.Attachments)
            {
                attachment.IsDeleted = true;
                attachment.DeletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Dépense supprimée avec succès" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting expense {ExpenseId}", id);
            return StatusCode(500, new { message = "Error deleting expense" });
        }
    }

    /// <summary>
    /// Résumé des dépenses par année (pour reporting)
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<ExpenseSummaryDto>> GetExpenseSummary([FromQuery] int year = 0)
    {
        try
        {
            if (year == 0)
            {
                year = DateTime.Now.Year;
            }

            var expenses = await _context.Expenses
                .Include(e => e.Supplier)
                .Where(e => e.ExpenseDate.Year == year)
                .ToListAsync();

            var totalYearAmount = expenses.Sum(e => e.Amount);

            var totalsByMonth = expenses
                .GroupBy(e => e.ExpenseDate.Month)
                .Select(g => new MonthlyExpenseDto
                {
                    Month = g.Key,
                    MonthName = CultureInfo.GetCultureInfo("fr-FR").DateTimeFormat.GetMonthName(g.Key),
                    Amount = g.Sum(e => e.Amount)
                })
                .OrderBy(m => m.Month)
                .ToList();

            var totalsByCategory = expenses
                .Where(e => e.Supplier != null)
                .GroupBy(e => e.Supplier.ServiceCategory)
                .Select(g => new CategoryExpenseDto
                {
                    CategoryId = 0,
                    CategoryName = g.Key ?? "Sans catégorie",
                    Amount = g.Sum(e => e.Amount)
                })
                .OrderByDescending(c => c.Amount)
                .ToList();

            return Ok(new ExpenseSummaryDto
            {
                Year = year,
                TotalYearAmount = totalYearAmount,
                TotalsByMonth = totalsByMonth,
                TotalsByCategory = totalsByCategory
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expense summary for year {Year}", year);
            return StatusCode(500, new { message = "Error retrieving summary" });
        }
    }

    // ========== ATTACHMENTS ==========

    /// <summary>
    /// Liste des pièces jointes d'une dépense
    /// </summary>
    [HttpGet("{expenseId}/attachments")]
    public async Task<ActionResult<List<AttachmentDto>>> GetAttachments(int expenseId)
    {
        try
        {
            var expenseExists = await _context.Expenses.AnyAsync(e => e.Id == expenseId);
            if (!expenseExists)
            {
                return NotFound(new { message = "Dépense non trouvée" });
            }

            var attachments = await _context.ExpenseAttachments
                .Include(a => a.UploadedBy)
                .Where(a => a.ExpenseId == expenseId)
                .OrderByDescending(a => a.UploadedAt)
                .Select(a => new AttachmentDto
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    FileType = a.FileType,
                    FileSize = a.FileSize,
                    UploadedAt = a.UploadedAt,
                    UploadedByName = a.UploadedBy.FirstName + " " + a.UploadedBy.LastName
                })
                .ToListAsync();

            return Ok(attachments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attachments for expense {ExpenseId}", expenseId);
            return StatusCode(500, new { message = "Error retrieving attachments" });
        }
    }

    /// <summary>
    /// Upload une pièce jointe
    /// </summary>
    [HttpPost("{expenseId}/attachments")]
    public async Task<ActionResult<AttachmentDto>> UploadAttachment(int expenseId, IFormFile file)
    {
        try
        {
            var expense = await _context.Expenses.FindAsync(expenseId);
            if (expense == null)
            {
                return NotFound(new { message = "Dépense non trouvée" });
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Aucun fichier fourni" });
            }

            // Validation type de fichier
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "application/pdf" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
            {
                return BadRequest(new { message = "Type de fichier non autorisé. Formats acceptés : JPG, PNG, PDF" });
            }

            // Validation taille (5MB)
            var maxSize = 5 * 1024 * 1024;
            if (file.Length > maxSize)
            {
                return BadRequest(new { message = "Fichier trop volumineux. Taille maximale : 5MB" });
            }

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Créer le répertoire si nécessaire
            var uploadsFolder = Path.Combine(_environment.ContentRootPath, "uploads", "expenses");
            Directory.CreateDirectory(uploadsFolder);

            // Générer un nom de fichier unique
            var fileExtension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{expenseId}_{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Sauvegarder le fichier
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Créer l'enregistrement dans la base
            var attachment = new ExpenseAttachment
            {
                ExpenseId = expenseId,
                FileName = file.FileName,
                FilePath = $"uploads/expenses/{uniqueFileName}",
                FileType = file.ContentType,
                FileSize = file.Length,
                UploadedByUserId = currentUserId,
                UploadedAt = DateTime.UtcNow
            };

            _context.ExpenseAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            var uploadedBy = await _context.Users.FindAsync(currentUserId);

            return CreatedAtAction(nameof(GetAttachments), new { expenseId }, new AttachmentDto
            {
                Id = attachment.Id,
                FileName = attachment.FileName,
                FileType = attachment.FileType,
                FileSize = attachment.FileSize,
                UploadedAt = attachment.UploadedAt,
                UploadedByName = uploadedBy != null ? $"{uploadedBy.FirstName} {uploadedBy.LastName}" : ""
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading attachment for expense {ExpenseId}", expenseId);
            return StatusCode(500, new { message = "Error uploading attachment" });
        }
    }

    /// <summary>
    /// Télécharger une pièce jointe
    /// </summary>
    [HttpGet("attachments/{attachmentId}/download")]
    public async Task<IActionResult> DownloadAttachment(int attachmentId)
    {
        try
        {
            var attachment = await _context.ExpenseAttachments.FindAsync(attachmentId);
            
            if (attachment == null)
            {
                return NotFound(new { message = "Pièce jointe non trouvée" });
            }

            var fullPath = Path.Combine(_environment.ContentRootPath, attachment.FilePath);
            
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound(new { message = "Fichier physique introuvable" });
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(fullPath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, attachment.FileType, attachment.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading attachment {AttachmentId}", attachmentId);
            return StatusCode(500, new { message = "Error downloading attachment" });
        }
    }

    /// <summary>
    /// Supprimer une pièce jointe (Soft Delete - SuperAdmin uniquement)
    /// </summary>
    [HttpDelete("{expenseId}/attachments/{attachmentId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteAttachment(int expenseId, int attachmentId)
    {
        try
        {
            var attachment = await _context.ExpenseAttachments
                .FirstOrDefaultAsync(a => a.Id == attachmentId && a.ExpenseId == expenseId);

            if (attachment == null)
            {
                return NotFound(new { message = "Pièce jointe non trouvée" });
            }

            // Soft delete de la pièce jointe
            attachment.IsDeleted = true;
            attachment.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Pièce jointe supprimée avec succès" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting attachment {AttachmentId}", attachmentId);
            return StatusCode(500, new { message = "Error deleting attachment" });
        }
    }

    // Helper methods
    private async Task<ExpenseDetailDto> GetExpenseDto(int id)
    {
        var expense = await _context.Expenses
            .Include(e => e.Supplier)
            .Include(e => e.RecordedBy)
            .Include(e => e.Attachments)
                .ThenInclude(a => a.UploadedBy)
            .FirstAsync(e => e.Id == id);

        return new ExpenseDetailDto
        {
            Id = expense.Id,
            ExpenseDate = expense.ExpenseDate,
            CategoryId = 0,
            CategoryName = expense.Supplier?.ServiceCategory,
            SupplierId = expense.SupplierId,
            SupplierName = expense.Supplier?.Name,
            Amount = expense.Amount,
            Description = expense.Description,
            InvoiceNumber = expense.InvoiceNumber,
            Notes = expense.Notes,
            RecordedAt = expense.RecordedAt,
            RecordedByName = expense.RecordedBy.FirstName + " " + expense.RecordedBy.LastName,
            Attachments = expense.Attachments.Select(a => new AttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                FileType = a.FileType,
                FileSize = a.FileSize,
                UploadedAt = a.UploadedAt,
                UploadedByName = a.UploadedBy.FirstName + " " + a.UploadedBy.LastName
            }).ToList()
        };
    }

    private void DeletePhysicalFile(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_environment.ContentRootPath, filePath);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not delete file {FilePath}", filePath);
        }
    }
}
