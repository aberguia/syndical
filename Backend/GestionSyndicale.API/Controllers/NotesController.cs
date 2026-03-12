using GestionSyndicale.Core.DTOs.Notes;
using GestionSyndicale.Core.Entities;
using GestionSyndicale.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GestionSyndicale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class NotesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NotesController> _logger;

    public NotesController(ApplicationDbContext context, ILogger<NotesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberNoteListDto>>> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] int? buildingId = null,
        [FromQuery] int? apartmentId = null)
    {
        try
        {
            var query = _context.MemberNotes
                .Include(n => n.Member)
                    .ThenInclude(m => m.Apartment)
                        .ThenInclude(a => a!.Building)
                .Include(n => n.CreatedByUser)
                .Where(n => !n.IsDeleted)
                .AsQueryable();

            // Filtres
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(n => 
                    (n.Member.FirstName + " " + n.Member.LastName).ToLower().Contains(search) ||
                    n.NoteText.ToLower().Contains(search));
            }

            if (buildingId.HasValue)
            {
                query = query.Where(n => n.Member.Apartment != null && n.Member.Apartment.BuildingId == buildingId.Value);
            }

            if (apartmentId.HasValue)
            {
                query = query.Where(n => n.Member.ApartmentId == apartmentId.Value);
            }

            var notes = await query
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new MemberNoteListDto
                {
                    Id = n.Id,
                    MemberId = n.MemberId,
                    MemberFullName = n.Member.FirstName + " " + n.Member.LastName,
                    BuildingId = n.Member.Apartment != null ? n.Member.Apartment.BuildingId : (int?)null,
                    BuildingCodeOrName = n.Member.Apartment != null ? n.Member.Apartment.Building.Name : null,
                    ApartmentId = n.Member.ApartmentId,
                    ApartmentNumber = n.Member.Apartment != null ? n.Member.Apartment.ApartmentNumber : null,
                    NoteText = n.NoteText,
                    CreatedAt = n.CreatedAt,
                    CreatedByName = n.CreatedByUser != null ? n.CreatedByUser.FirstName + " " + n.CreatedByUser.LastName : null
                })
                .ToListAsync();

            return Ok(notes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving member notes");
            return StatusCode(500, new { message = "Error retrieving member notes" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MemberNoteListDto>> GetById(int id)
    {
        try
        {
            var note = await _context.MemberNotes
                .Include(n => n.Member)
                    .ThenInclude(m => m.Apartment)
                        .ThenInclude(a => a!.Building)
                .Include(n => n.CreatedByUser)
                .Where(n => n.Id == id && !n.IsDeleted)
                .Select(n => new MemberNoteListDto
                {
                    Id = n.Id,
                    MemberId = n.MemberId,
                    MemberFullName = n.Member.FirstName + " " + n.Member.LastName,
                    BuildingId = n.Member.Apartment != null ? n.Member.Apartment.BuildingId : (int?)null,
                    BuildingCodeOrName = n.Member.Apartment != null ? n.Member.Apartment.Building.Name : null,
                    ApartmentId = n.Member.ApartmentId,
                    ApartmentNumber = n.Member.Apartment != null ? n.Member.Apartment.ApartmentNumber : null,
                    NoteText = n.NoteText,
                    CreatedAt = n.CreatedAt,
                    CreatedByName = n.CreatedByUser != null ? n.CreatedByUser.FirstName + " " + n.CreatedByUser.LastName : null
                })
                .FirstOrDefaultAsync();

            if (note == null)
                return NotFound(new { message = "Note not found" });

            return Ok(note);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving note {NoteId}", id);
            return StatusCode(500, new { message = "Error retrieving note" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<MemberNoteListDto>> Create([FromBody] CreateMemberNoteDto dto)
    {
        try
        {
            // Vérifier que le membre existe
            var member = await _context.Users.FindAsync(dto.MemberId);
            if (member == null || member.IsDeleted)
            {
                return BadRequest(new { message = "Adhérent introuvable" });
            }

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var note = new MemberNote
            {
                MemberId = dto.MemberId,
                NoteText = dto.NoteText,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = currentUserId,
                IsDeleted = false
            };

            _context.MemberNotes.Add(note);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = note.Id }, await GetById(note.Id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating member note");
            return StatusCode(500, new { message = "Error creating member note" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MemberNoteListDto>> Update(int id, [FromBody] UpdateMemberNoteDto dto)
    {
        try
        {
            var note = await _context.MemberNotes.FindAsync(id);
            if (note == null || note.IsDeleted)
                return NotFound(new { message = "Note not found" });

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            note.NoteText = dto.NoteText;
            note.UpdatedAt = DateTime.UtcNow;
            note.UpdatedByUserId = currentUserId;

            await _context.SaveChangesAsync();

            return Ok(await GetById(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating note {NoteId}", id);
            return StatusCode(500, new { message = "Error updating note" });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var note = await _context.MemberNotes.FindAsync(id);
            if (note == null || note.IsDeleted)
                return NotFound(new { message = "Note not found" });

            note.IsDeleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting note {NoteId}", id);
            return StatusCode(500, new { message = "Error deleting note" });
        }
    }

    [HttpGet("members/lookup")]
    public async Task<ActionResult<IEnumerable<MemberLookupForNotesDto>>> GetMembersLookup(
        [FromQuery] int? buildingId = null,
        [FromQuery] int? apartmentId = null)
    {
        try
        {
            var query = _context.Users
                .Include(u => u.Apartment)
                    .ThenInclude(a => a!.Building)
                .Where(u => !u.IsDeleted && u.ApartmentId != null)
                .AsQueryable();

            if (buildingId.HasValue)
            {
                query = query.Where(u => u.Apartment!.BuildingId == buildingId.Value);
            }

            if (apartmentId.HasValue)
            {
                query = query.Where(u => u.ApartmentId == apartmentId.Value);
            }

            var members = await query
                .Select(u => new MemberLookupForNotesDto
                {
                    MemberId = u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    ApartmentNumber = u.Apartment!.ApartmentNumber,
                    BuildingCodeOrName = u.Apartment.Building.Name,
                    DisplayText = $"{u.FirstName} {u.LastName} — {u.Apartment.Building.Name} — Appt {u.Apartment.ApartmentNumber}"
                })
                .OrderBy(m => m.FullName)
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
