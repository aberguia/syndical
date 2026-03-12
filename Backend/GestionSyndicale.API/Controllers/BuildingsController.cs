using GestionSyndicale.Core.DTOs.Settings;
using GestionSyndicale.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionSyndicale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class BuildingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BuildingsController> _logger;

    public BuildingsController(ApplicationDbContext context, ILogger<BuildingsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Récupère la liste de tous les immeubles
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<BuildingDto>>> GetAll()
    {
        try
        {
            var buildings = await _context.Buildings
                .Include(b => b.Apartments)
                .OrderBy(b => b.BuildingNumber)
                .Select(b => new BuildingDto
                {
                    Id = b.Id,
                    ResidenceId = b.ResidenceId,
                    BuildingNumber = b.BuildingNumber,
                    Name = b.Name,
                    FloorCount = b.FloorCount,
                    IsActive = b.IsActive,
                    ApartmentsCount = b.Apartments.Count,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt
                })
                .ToListAsync();

            return Ok(buildings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des immeubles");
            return StatusCode(500, new { message = "Erreur serveur lors de la récupération des immeubles" });
        }
    }

    /// <summary>
    /// Récupère un immeuble par son ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<BuildingDto>> GetById(int id)
    {
        try
        {
            var building = await _context.Buildings
                .Include(b => b.Apartments)
                .Where(b => b.Id == id)
                .Select(b => new BuildingDto
                {
                    Id = b.Id,
                    ResidenceId = b.ResidenceId,
                    BuildingNumber = b.BuildingNumber,
                    Name = b.Name,
                    FloorCount = b.FloorCount,
                    IsActive = b.IsActive,
                    ApartmentsCount = b.Apartments.Count,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (building == null)
            {
                return NotFound(new { message = "Immeuble introuvable" });
            }

            return Ok(building);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de l'immeuble {BuildingId}", id);
            return StatusCode(500, new { message = "Erreur serveur lors de la récupération de l'immeuble" });
        }
    }

    /// <summary>
    /// Crée un nouvel immeuble
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BuildingDto>> Create([FromBody] CreateBuildingDto dto)
    {
        try
        {
            // Récupérer la résidence par défaut (instance unique)
            var residence = await _context.Residences.FirstOrDefaultAsync();
            if (residence == null)
            {
                return BadRequest(new { message = "Aucune résidence configurée" });
            }

            // Vérifier l'unicité du code immeuble
            var exists = await _context.Buildings
                .AnyAsync(b => b.ResidenceId == residence.Id && b.BuildingNumber == dto.BuildingNumber);

            if (exists)
            {
                return Conflict(new { message = $"Un immeuble avec le code '{dto.BuildingNumber}' existe déjà" });
            }

            var building = new Core.Entities.Building
            {
                ResidenceId = residence.Id,
                BuildingNumber = dto.BuildingNumber,
                Name = dto.Name,
                FloorCount = dto.FloorCount,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Buildings.Add(building);
            await _context.SaveChangesAsync();

            var result = new BuildingDto
            {
                Id = building.Id,
                ResidenceId = building.ResidenceId,
                BuildingNumber = building.BuildingNumber,
                Name = building.Name,
                FloorCount = building.FloorCount,
                IsActive = building.IsActive,
                ApartmentsCount = 0,
                CreatedAt = building.CreatedAt,
                UpdatedAt = building.UpdatedAt
            };

            _logger.LogInformation("Immeuble créé : {BuildingNumber}", building.BuildingNumber);
            return CreatedAtAction(nameof(GetById), new { id = building.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création de l'immeuble");
            return StatusCode(500, new { message = "Erreur serveur lors de la création de l'immeuble" });
        }
    }

    /// <summary>
    /// Modifie un immeuble existant
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<BuildingDto>> Update(int id, [FromBody] UpdateBuildingDto dto)
    {
        try
        {
            var building = await _context.Buildings
                .Include(b => b.Apartments)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (building == null)
            {
                return NotFound(new { message = "Immeuble introuvable" });
            }

            // Vérifier l'unicité du code immeuble (sauf pour l'immeuble actuel)
            var exists = await _context.Buildings
                .AnyAsync(b => b.ResidenceId == building.ResidenceId && 
                              b.BuildingNumber == dto.BuildingNumber && 
                              b.Id != id);

            if (exists)
            {
                return Conflict(new { message = $"Un immeuble avec le code '{dto.BuildingNumber}' existe déjà" });
            }

            building.BuildingNumber = dto.BuildingNumber;
            building.Name = dto.Name;
            building.FloorCount = dto.FloorCount;
            building.IsActive = dto.IsActive;
            building.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = new BuildingDto
            {
                Id = building.Id,
                ResidenceId = building.ResidenceId,
                BuildingNumber = building.BuildingNumber,
                Name = building.Name,
                FloorCount = building.FloorCount,
                IsActive = building.IsActive,
                ApartmentsCount = building.Apartments.Count,
                CreatedAt = building.CreatedAt,
                UpdatedAt = building.UpdatedAt
            };

            _logger.LogInformation("Immeuble modifié : {BuildingId}", id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la modification de l'immeuble {BuildingId}", id);
            return StatusCode(500, new { message = "Erreur serveur lors de la modification de l'immeuble" });
        }
    }

    /// <summary>
    /// Supprime un immeuble
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var building = await _context.Buildings
                .Include(b => b.Apartments)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (building == null)
            {
                return NotFound(new { message = "Immeuble introuvable" });
            }

            // Vérifier qu'il n'y a pas d'appartements liés
            if (building.Apartments.Any())
            {
                return BadRequest(new 
                { 
                    message = "Impossible de supprimer cet immeuble car il contient des appartements",
                    apartmentsCount = building.Apartments.Count
                });
            }

            _context.Buildings.Remove(building);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Immeuble supprimé : {BuildingId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de l'immeuble {BuildingId}", id);
            return StatusCode(500, new { message = "Erreur serveur lors de la suppression de l'immeuble" });
        }
    }
}
