using GestionSyndicale.Core.DTOs.Settings;
using GestionSyndicale.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionSyndicale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class ApartmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ApartmentsController> _logger;

    public ApartmentsController(ApplicationDbContext context, ILogger<ApartmentsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Récupère la liste de tous les appartements (avec filtre optionnel par immeuble)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ApartmentDto>>> GetAll([FromQuery] int? buildingId = null)
    {
        try
        {
            var query = _context.Apartments
                .Include(a => a.Building)
                .AsQueryable();

            if (buildingId.HasValue)
            {
                query = query.Where(a => a.BuildingId == buildingId.Value);
            }

            var apartments = await query
                .OrderBy(a => a.Building.BuildingNumber)
                .ThenBy(a => a.ApartmentNumber)
                .Select(a => new ApartmentDto
                {
                    Id = a.Id,
                    BuildingId = a.BuildingId,
                    BuildingNumber = a.Building.BuildingNumber,
                    BuildingName = a.Building.Name,
                    ApartmentNumber = a.ApartmentNumber,
                    Floor = a.Floor,
                    Surface = a.Surface,
                    SharesCount = a.SharesCount,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt,
                    MemberFullName = _context.Users
                        .Where(u => u.ApartmentId == a.Id && !u.IsDeleted && u.IsActive)
                        .Select(u => u.FirstName + " " + u.LastName)
                        .FirstOrDefault(),
                    MemberId = _context.Users
                        .Where(u => u.ApartmentId == a.Id && !u.IsDeleted && u.IsActive)
                        .Select(u => u.Id)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(apartments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des appartements");
            return StatusCode(500, new { message = "Erreur serveur lors de la récupération des appartements" });
        }
    }

    /// <summary>
    /// Récupère un appartement par son ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApartmentDto>> GetById(int id)
    {
        try
        {
            var apartment = await _context.Apartments
                .Include(a => a.Building)
                .Where(a => a.Id == id)
                .Select(a => new ApartmentDto
                {
                    Id = a.Id,
                    BuildingId = a.BuildingId,
                    BuildingNumber = a.Building.BuildingNumber,
                    BuildingName = a.Building.Name,
                    ApartmentNumber = a.ApartmentNumber,
                    Floor = a.Floor,
                    Surface = a.Surface,
                    SharesCount = a.SharesCount,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (apartment == null)
            {
                return NotFound(new { message = "Appartement introuvable" });
            }

            return Ok(apartment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de l'appartement {ApartmentId}", id);
            return StatusCode(500, new { message = "Erreur serveur lors de la récupération de l'appartement" });
        }
    }

    /// <summary>
    /// Crée un nouvel appartement
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApartmentDto>> Create([FromBody] CreateApartmentDto dto)
    {
        try
        {
            // Vérifier que l'immeuble existe
            var building = await _context.Buildings.FindAsync(dto.BuildingId);
            if (building == null)
            {
                return BadRequest(new { message = "Immeuble introuvable" });
            }

            // Vérifier l'unicité (BuildingId + ApartmentNumber)
            var exists = await _context.Apartments
                .AnyAsync(a => a.BuildingId == dto.BuildingId && a.ApartmentNumber == dto.ApartmentNumber);

            if (exists)
            {
                return Conflict(new 
                { 
                    message = $"L'appartement {dto.ApartmentNumber} existe déjà dans l'immeuble {building.BuildingNumber}" 
                });
            }

            var apartment = new Core.Entities.Apartment
            {
                BuildingId = dto.BuildingId,
                ApartmentNumber = dto.ApartmentNumber,
                Floor = dto.Floor,
                Surface = dto.Surface,
                SharesCount = dto.SharesCount,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Apartments.Add(apartment);
            await _context.SaveChangesAsync();

            // Recharger avec le Building pour la réponse
            await _context.Entry(apartment).Reference(a => a.Building).LoadAsync();

            var result = new ApartmentDto
            {
                Id = apartment.Id,
                BuildingId = apartment.BuildingId,
                BuildingNumber = apartment.Building.BuildingNumber,
                BuildingName = apartment.Building.Name,
                ApartmentNumber = apartment.ApartmentNumber,
                Floor = apartment.Floor,
                Surface = apartment.Surface,
                SharesCount = apartment.SharesCount,
                IsActive = apartment.IsActive,
                CreatedAt = apartment.CreatedAt,
                UpdatedAt = apartment.UpdatedAt
            };

            _logger.LogInformation("Appartement créé : {BuildingNumber}-{ApartmentNumber}", 
                apartment.Building.BuildingNumber, apartment.ApartmentNumber);
            
            return CreatedAtAction(nameof(GetById), new { id = apartment.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création de l'appartement");
            return StatusCode(500, new { message = "Erreur serveur lors de la création de l'appartement" });
        }
    }

    /// <summary>
    /// Modifie un appartement existant
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApartmentDto>> Update(int id, [FromBody] UpdateApartmentDto dto)
    {
        try
        {
            var apartment = await _context.Apartments
                .Include(a => a.Building)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (apartment == null)
            {
                return NotFound(new { message = "Appartement introuvable" });
            }

            // Si l'immeuble change, vérifier qu'il existe
            if (apartment.BuildingId != dto.BuildingId)
            {
                var building = await _context.Buildings.FindAsync(dto.BuildingId);
                if (building == null)
                {
                    return BadRequest(new { message = "Immeuble introuvable" });
                }
            }

            // Vérifier l'unicité (BuildingId + ApartmentNumber) excluant l'appartement actuel
            var exists = await _context.Apartments
                .AnyAsync(a => a.BuildingId == dto.BuildingId && 
                              a.ApartmentNumber == dto.ApartmentNumber && 
                              a.Id != id);

            if (exists)
            {
                var buildingNumber = await _context.Buildings
                    .Where(b => b.Id == dto.BuildingId)
                    .Select(b => b.BuildingNumber)
                    .FirstOrDefaultAsync();

                return Conflict(new 
                { 
                    message = $"L'appartement {dto.ApartmentNumber} existe déjà dans l'immeuble {buildingNumber}" 
                });
            }

            apartment.BuildingId = dto.BuildingId;
            apartment.ApartmentNumber = dto.ApartmentNumber;
            apartment.Floor = dto.Floor;
            apartment.Surface = dto.Surface;
            apartment.SharesCount = dto.SharesCount;
            apartment.IsActive = dto.IsActive;
            apartment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Recharger le Building si changé
            if (_context.Entry(apartment).Reference(a => a.Building).CurrentValue?.Id != apartment.BuildingId)
            {
                await _context.Entry(apartment).Reference(a => a.Building).LoadAsync();
            }

            var result = new ApartmentDto
            {
                Id = apartment.Id,
                BuildingId = apartment.BuildingId,
                BuildingNumber = apartment.Building.BuildingNumber,
                BuildingName = apartment.Building.Name,
                ApartmentNumber = apartment.ApartmentNumber,
                Floor = apartment.Floor,
                Surface = apartment.Surface,
                SharesCount = apartment.SharesCount,
                IsActive = apartment.IsActive,
                CreatedAt = apartment.CreatedAt,
                UpdatedAt = apartment.UpdatedAt
            };

            _logger.LogInformation("Appartement modifié : {ApartmentId}", id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la modification de l'appartement {ApartmentId}", id);
            return StatusCode(500, new { message = "Erreur serveur lors de la modification de l'appartement" });
        }
    }

    /// <summary>
    /// Supprime un appartement
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var apartment = await _context.Apartments
                .Include(a => a.PrimaryOwner)
                .Include(a => a.Payments)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (apartment == null)
            {
                return NotFound(new { message = "Appartement introuvable" });
            }

            // Vérifier qu'il n'y a pas d'utilisateur associé
            if (apartment.PrimaryOwner != null)
            {
                return BadRequest(new 
                { 
                    message = "Impossible de supprimer cet appartement car un copropriétaire y est associé",
                    ownerEmail = apartment.PrimaryOwner.Email
                });
            }

            // Vérifier qu'il n'y a pas de paiements associés
            if (apartment.Payments.Any())
            {
                return BadRequest(new 
                { 
                    message = "Impossible de supprimer cet appartement car des paiements y sont associés",
                    paymentsCount = apartment.Payments.Count
                });
            }

            _context.Apartments.Remove(apartment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Appartement supprimé : {ApartmentId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de l'appartement {ApartmentId}", id);
            return StatusCode(500, new { message = "Erreur serveur lors de la suppression de l'appartement" });
        }
    }

    /// <summary>
    /// Récupère le nom de l'adhérent associé à un appartement
    /// </summary>
    [HttpGet("{id}/member-name")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ApartmentMemberNameDto>> GetMemberName(int id)
    {
        try
        {
            var apartment = await _context.Apartments
                .Include(a => a.PrimaryOwner)
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            if (apartment == null)
            {
                return NotFound(new { message = "Appartement introuvable" });
            }

            var memberName = apartment.PrimaryOwner != null
                ? $"{apartment.PrimaryOwner.FirstName} {apartment.PrimaryOwner.LastName}"
                : null;

            return Ok(new ApartmentMemberNameDto
            {
                ApartmentId = id,
                MemberName = memberName,
                HasMember = apartment.PrimaryOwner != null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de l'adhérent pour l'appartement {ApartmentId}", id);
            return StatusCode(500, new { message = "Erreur serveur" });
        }
    }
}

public class ApartmentMemberNameDto
{
    public int ApartmentId { get; set; }
    public string? MemberName { get; set; }
    public bool HasMember { get; set; }
}
