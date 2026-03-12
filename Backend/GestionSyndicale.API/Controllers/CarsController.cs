using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GestionSyndicale.Infrastructure.Data;
using GestionSyndicale.Core.DTOs.Parking;
using GestionSyndicale.Core.Entities;

namespace GestionSyndicale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class CarsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CarsController> _logger;

    public CarsController(ApplicationDbContext context, ILogger<CarsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<CarListDto>>> GetAll([FromQuery] string? search = null, [FromQuery] int? buildingId = null)
    {
        try
        {
            var query = _context.Cars
                .Include(c => c.Member)
                    .ThenInclude(m => m.Apartment)
                        .ThenInclude(a => a!.Building)
                .Where(c => !c.IsDeleted)
                .AsQueryable();

            // Filtre par immeuble
            if (buildingId.HasValue)
            {
                query = query.Where(c => c.Member.Apartment!.BuildingId == buildingId.Value);
            }

            // Filtre par recherche (plaque ou nom membre)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchNormalized = search.Replace(" ", "").Replace("-", "").Replace("|", "").ToLower();
                query = query.Where(c =>
                    (c.PlatePart1.ToString() + c.PlatePart2 + c.PlatePart3.ToString()).ToLower().Contains(searchNormalized) ||
                    (c.Member.FirstName + " " + c.Member.LastName).ToLower().Contains(search.ToLower())
                );
            }

            var cars = await query
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CarListDto
                {
                    Id = c.Id,
                    Brand = c.Brand,
                    PlatePart1 = c.PlatePart1,
                    PlatePart2 = c.PlatePart2,
                    PlatePart3 = c.PlatePart3,
                    CarType = c.CarType,
                    MemberId = c.MemberId,
                    MemberFullName = c.Member.FirstName + " " + c.Member.LastName,
                    MemberPhone = c.Member.Phone,
                    BuildingId = c.Member.Apartment != null ? c.Member.Apartment.BuildingId : (int?)null,
                    BuildingCode = c.Member.Apartment != null ? c.Member.Apartment.Building.BuildingNumber : null,
                    ApartmentId = c.Member.ApartmentId,
                    ApartmentNumber = c.Member.Apartment != null ? c.Member.Apartment.ApartmentNumber : null,
                    Notes = c.Notes,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(cars);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cars");
            return StatusCode(500, new { message = "Error retrieving cars" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CarListDto>> GetById(int id)
    {
        try
        {
            var car = await _context.Cars
                .Include(c => c.Member)
                    .ThenInclude(m => m.Apartment)
                        .ThenInclude(a => a!.Building)
                .Where(c => c.Id == id && !c.IsDeleted)
                .Select(c => new CarListDto
                {
                    Id = c.Id,
                    Brand = c.Brand,
                    PlatePart1 = c.PlatePart1,
                    PlatePart2 = c.PlatePart2,
                    PlatePart3 = c.PlatePart3,
                    CarType = c.CarType,
                    MemberId = c.MemberId,
                    MemberFullName = c.Member.FirstName + " " + c.Member.LastName,
                    MemberPhone = c.Member.Phone,
                    BuildingId = c.Member.Apartment != null ? c.Member.Apartment.BuildingId : (int?)null,
                    BuildingCode = c.Member.Apartment != null ? c.Member.Apartment.Building.BuildingNumber : null,
                    ApartmentId = c.Member.ApartmentId,
                    ApartmentNumber = c.Member.Apartment != null ? c.Member.Apartment.ApartmentNumber : null,
                    Notes = c.Notes,
                    CreatedAt = c.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (car == null)
                return NotFound(new { message = "Car not found" });

            return Ok(car);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving car {CarId}", id);
            return StatusCode(500, new { message = "Error retrieving car" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<CarListDto>> Create([FromBody] CreateCarDto dto)
    {
        try
        {
            // Vérifier unicité plaque
            var existingCar = await _context.Cars
                .AnyAsync(c => c.PlatePart1 == dto.PlatePart1 &&
                              c.PlatePart2 == dto.PlatePart2 &&
                              c.PlatePart3 == dto.PlatePart3 &&
                              !c.IsDeleted);

            if (existingCar)
            {
                return BadRequest(new { message = $"Une voiture avec la plaque {dto.PlatePart1} {dto.PlatePart2} {dto.PlatePart3} existe déjà" });
            }

            // Vérifier que le membre existe
            var member = await _context.Users.FindAsync(dto.MemberId);
            if (member == null || member.IsDeleted)
            {
                return BadRequest(new { message = "Adhérent introuvable" });
            }

            var car = new Car
            {
                Brand = dto.Brand,
                PlatePart1 = dto.PlatePart1,
                PlatePart2 = dto.PlatePart2.ToUpper(),
                PlatePart3 = dto.PlatePart3,
                CarType = dto.CarType,
                MemberId = dto.MemberId,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Cars.Add(car);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = car.Id }, await GetById(car.Id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating car");
            return StatusCode(500, new { message = "Error creating car" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CarListDto>> Update(int id, [FromBody] UpdateCarDto dto)
    {
        try
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null || car.IsDeleted)
                return NotFound(new { message = "Car not found" });

            // Vérifier unicité plaque (sauf pour cette voiture)
            var existingCar = await _context.Cars
                .AnyAsync(c => c.Id != id &&
                              c.PlatePart1 == dto.PlatePart1 &&
                              c.PlatePart2 == dto.PlatePart2 &&
                              c.PlatePart3 == dto.PlatePart3 &&
                              !c.IsDeleted);

            if (existingCar)
            {
                return BadRequest(new { message = $"Une voiture avec la plaque {dto.PlatePart1} {dto.PlatePart2} {dto.PlatePart3} existe déjà" });
            }

            // Vérifier que le membre existe
            var member = await _context.Users.FindAsync(dto.MemberId);
            if (member == null || member.IsDeleted)
            {
                return BadRequest(new { message = "Adhérent introuvable" });
            }

            car.Brand = dto.Brand;
            car.PlatePart1 = dto.PlatePart1;
            car.PlatePart2 = dto.PlatePart2.ToUpper();
            car.PlatePart3 = dto.PlatePart3;
            car.CarType = dto.CarType;
            car.MemberId = dto.MemberId;
            car.Notes = dto.Notes;
            car.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(await GetById(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating car {CarId}", id);
            return StatusCode(500, new { message = "Error updating car" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null || car.IsDeleted)
                return NotFound(new { message = "Car not found" });

            car.IsDeleted = true;
            car.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Voiture supprimée avec succès" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting car {CarId}", id);
            return StatusCode(500, new { message = "Error deleting car" });
        }
    }

    [HttpGet("members/lookup")]
    public async Task<ActionResult<List<MemberLookupDto>>> GetMembersLookup([FromQuery] int? buildingId = null)
    {
        try
        {
            var query = _context.Users
                .Include(u => u.Apartment)
                    .ThenInclude(a => a!.Building)
                .Where(u => !u.IsDeleted && u.IsActive && u.ApartmentId != null)
                .AsQueryable();

            if (buildingId.HasValue)
            {
                query = query.Where(u => u.Apartment!.BuildingId == buildingId.Value);
            }

            var members = await query
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Select(u => new MemberLookupDto
                {
                    Id = u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    BuildingCode = u.Apartment!.Building.BuildingNumber,
                    ApartmentNumber = u.Apartment!.ApartmentNumber
                })
                .ToListAsync();

            return Ok(members);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving members lookup");
            return StatusCode(500, new { message = "Error retrieving members" });
        }
    }
}
