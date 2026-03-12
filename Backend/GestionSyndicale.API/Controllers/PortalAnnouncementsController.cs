using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionSyndicale.Core.DTOs.Announcements;
using GestionSyndicale.Infrastructure.Data;

namespace GestionSyndicale.API.Controllers;

[Authorize]
[ApiController]
[Route("api/portal/announcements")]
public class PortalAnnouncementsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PortalAnnouncementsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<AnnouncementDto>>> GetPublished([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var announcements = await _context.Announcements
            .Where(a => a.Status == Core.Entities.AnnouncementStatus.Published)
            .OrderByDescending(a => a.CreatedOn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(a => a.CreatedBy)
            .Select(a => new AnnouncementDto
            {
                Id = a.Id,
                Title = a.Title,
                Body = a.Body,
                Status = a.Status.ToString(),
                CreatedByName = a.CreatedBy.FirstName + " " + a.CreatedBy.LastName,
                CreatedOn = a.CreatedOn
            })
            .ToListAsync();

        return Ok(announcements);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AnnouncementDto>> GetById(int id)
    {
        var announcement = await _context.Announcements
            .Where(a => a.Id == id && a.Status == Core.Entities.AnnouncementStatus.Published)
            .Include(a => a.CreatedBy)
            .FirstOrDefaultAsync();

        if (announcement == null) return NotFound();

        return Ok(new AnnouncementDto
        {
            Id = announcement.Id,
            Title = announcement.Title,
            Body = announcement.Body,
            Status = announcement.Status.ToString(),
            CreatedByName = announcement.CreatedBy.FirstName + " " + announcement.CreatedBy.LastName,
            CreatedOn = announcement.CreatedOn
        });
    }
}
