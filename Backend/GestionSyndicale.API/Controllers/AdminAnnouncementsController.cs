using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionSyndicale.Core.DTOs.Announcements;
using GestionSyndicale.Core.Entities;
using GestionSyndicale.Infrastructure.Data;
using System.Security.Claims;

namespace GestionSyndicale.API.Controllers;

[Authorize(Roles = "SuperAdmin,Admin")]
[ApiController]
[Route("api/admin/announcements")]
public class AdminAnnouncementsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminAnnouncementsController> _logger;

    public AdminAnnouncementsController(ApplicationDbContext context, ILogger<AdminAnnouncementsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<AnnouncementListDto>> GetAll([FromQuery] string? status, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = _context.Announcements
                .Include(a => a.CreatedBy)
                .Include(a => a.UpdatedBy)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<AnnouncementStatus>(status, out var statusEnum))
            {
                query = query.Where(a => a.Status == statusEnum);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a => a.Title.Contains(search) || a.Body.Contains(search));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(a => a.CreatedOn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AnnouncementDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Body = a.Body,
                    Status = a.Status.ToString(),
                    CreatedByName = a.CreatedBy.FirstName + " " + a.CreatedBy.LastName,
                    CreatedOn = a.CreatedOn,
                    UpdatedByName = a.UpdatedBy != null ? a.UpdatedBy.FirstName + " " + a.UpdatedBy.LastName : null,
                    UpdatedOn = a.UpdatedOn
                })
                .ToListAsync();

            return Ok(new AnnouncementListDto { TotalCount = totalCount, Items = items });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting announcements");
            return StatusCode(500, "Error retrieving announcements");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AnnouncementDto>> GetById(int id)
    {
        var announcement = await _context.Announcements
            .Include(a => a.CreatedBy)
            .Include(a => a.UpdatedBy)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (announcement == null) return NotFound();

        return Ok(new AnnouncementDto
        {
            Id = announcement.Id,
            Title = announcement.Title,
            Body = announcement.Body,
            Status = announcement.Status.ToString(),
            CreatedByName = announcement.CreatedBy.FirstName + " " + announcement.CreatedBy.LastName,
            CreatedOn = announcement.CreatedOn,
            UpdatedByName = announcement.UpdatedBy != null ? announcement.UpdatedBy.FirstName + " " + announcement.UpdatedBy.LastName : null,
            UpdatedOn = announcement.UpdatedOn
        });
    }

    [HttpPost]
    public async Task<ActionResult<AnnouncementDto>> Create([FromBody] CreateAnnouncementDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var announcement = new Announcement
        {
            Title = dto.Title,
            Body = dto.Body,
            Status = AnnouncementStatus.Draft,
            CreatedByUserId = userId,
            CreatedOn = DateTime.UtcNow
        };

        _context.Announcements.Add(announcement);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = announcement.Id }, await GetAnnouncementDto(announcement.Id));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AnnouncementDto>> Update(int id, [FromBody] UpdateAnnouncementDto dto)
    {
        var announcement = await _context.Announcements.FindAsync(id);
        if (announcement == null) return NotFound();

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        announcement.Title = dto.Title;
        announcement.Body = dto.Body;
        announcement.UpdatedByUserId = userId;
        announcement.UpdatedOn = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(await GetAnnouncementDto(id));
    }

    [HttpPost("{id}/publish")]
    public async Task<IActionResult> Publish(int id)
    {
        var announcement = await _context.Announcements.FindAsync(id);
        if (announcement == null) return NotFound();

        announcement.Status = AnnouncementStatus.Published;
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("{id}/archive")]
    public async Task<IActionResult> Archive(int id)
    {
        var announcement = await _context.Announcements.FindAsync(id);
        if (announcement == null) return NotFound();

        announcement.Status = AnnouncementStatus.Archived;
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var announcement = await _context.Announcements.FindAsync(id);
        if (announcement == null) return NotFound();

        _context.Announcements.Remove(announcement);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<AnnouncementDto> GetAnnouncementDto(int id)
    {
        var announcement = await _context.Announcements
            .Include(a => a.CreatedBy)
            .Include(a => a.UpdatedBy)
            .FirstAsync(a => a.Id == id);

        return new AnnouncementDto
        {
            Id = announcement.Id,
            Title = announcement.Title,
            Body = announcement.Body,
            Status = announcement.Status.ToString(),
            CreatedByName = announcement.CreatedBy.FirstName + " " + announcement.CreatedBy.LastName,
            CreatedOn = announcement.CreatedOn,
            UpdatedByName = announcement.UpdatedBy != null ? announcement.UpdatedBy.FirstName + " " + announcement.UpdatedBy.LastName : null,
            UpdatedOn = announcement.UpdatedOn
        };
    }
}
