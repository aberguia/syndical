using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionSyndicale.Core.DTOs.Polls;
using GestionSyndicale.Core.Entities;
using GestionSyndicale.Infrastructure.Data;
using System.Security.Claims;

namespace GestionSyndicale.API.Controllers;

[Authorize(Roles = "SuperAdmin,Admin")]
[ApiController]
[Route("api/admin/polls")]
public class AdminPollsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminPollsController> _logger;

    public AdminPollsController(ApplicationDbContext context, ILogger<AdminPollsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PollListDto>> GetAll([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.Polls.Include(p => p.CreatedBy).Include(p => p.Options).Include(p => p.Votes).AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<PollStatus>(status, out var statusEnum))
        {
            query = query.Where(p => p.Status == statusEnum);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.CreatedOn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PollDto
            {
                Id = p.Id,
                Question = p.Question,
                Status = p.Status.ToString(),
                CreatedByName = p.CreatedBy.FirstName + " " + p.CreatedBy.LastName,
                CreatedOn = p.CreatedOn,
                ClosedOn = p.ClosedOn,
                Options = p.Options.OrderBy(o => o.SortOrder).Select(o => new PollOptionDto
                {
                    Id = o.Id,
                    Label = o.Label,
                    SortOrder = o.SortOrder
                }).ToList(),
                Results = p.Options.Select(o => new PollResultDto
                {
                    OptionId = o.Id,
                    Label = o.Label,
                    VoteCount = o.Votes.Count,
                    Percentage = p.Votes.Count > 0 ? (decimal)o.Votes.Count / p.Votes.Count * 100 : 0
                }).ToList()
            })
            .ToListAsync();

        return Ok(new PollListDto { TotalCount = totalCount, Items = items });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PollDto>> GetById(int id)
    {
        var poll = await _context.Polls
            .Include(p => p.CreatedBy)
            .Include(p => p.Options)
            .Include(p => p.Votes)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (poll == null) return NotFound();

        return Ok(MapToPollDto(poll));
    }

    [HttpPost]
    public async Task<ActionResult<PollDto>> Create([FromBody] CreatePollDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var poll = new Poll
        {
            Question = dto.Question,
            Status = PollStatus.Draft,
            CreatedByUserId = userId,
            CreatedOn = DateTime.UtcNow,
            Options = dto.Options.Select(o => new PollOption
            {
                Label = o.Label,
                SortOrder = o.SortOrder
            }).ToList()
        };

        _context.Polls.Add(poll);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = poll.Id }, await GetPollDtoById(poll.Id));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PollDto>> Update(int id, [FromBody] UpdatePollDto dto)
    {
        var poll = await _context.Polls.Include(p => p.Options).FirstOrDefaultAsync(p => p.Id == id);
        if (poll == null) return NotFound();
        if (poll.Status != PollStatus.Draft) return BadRequest("Can only update draft polls");

        poll.Question = dto.Question;

        // Remove old options
        _context.PollOptions.RemoveRange(poll.Options);

        // Add new options
        poll.Options = dto.Options.Select(o => new PollOption
        {
            PollId = poll.Id,
            Label = o.Label,
            SortOrder = o.SortOrder
        }).ToList();

        await _context.SaveChangesAsync();

        return Ok(await GetPollDtoById(id));
    }

    [HttpPost("{id}/publish")]
    public async Task<IActionResult> Publish(int id)
    {
        var poll = await _context.Polls.FindAsync(id);
        if (poll == null) return NotFound();

        poll.Status = PollStatus.Published;
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("{id}/close")]
    public async Task<IActionResult> Close(int id)
    {
        var poll = await _context.Polls.FindAsync(id);
        if (poll == null) return NotFound();

        poll.Status = PollStatus.Closed;
        poll.ClosedOn = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("{id}/results")]
    public async Task<ActionResult<List<PollResultDto>>> GetResults(int id)
    {
        var poll = await _context.Polls
            .Include(p => p.Options)
            .ThenInclude(o => o.Votes)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (poll == null) return NotFound();

        var totalVotes = poll.Votes.Count;
        var results = poll.Options.Select(o => new PollResultDto
        {
            OptionId = o.Id,
            Label = o.Label,
            VoteCount = o.Votes.Count,
            Percentage = totalVotes > 0 ? (decimal)o.Votes.Count / totalVotes * 100 : 0
        }).ToList();

        return Ok(results);
    }

    private async Task<PollDto> GetPollDtoById(int id)
    {
        var poll = await _context.Polls
            .Include(p => p.CreatedBy)
            .Include(p => p.Options)
            .Include(p => p.Votes)
            .FirstAsync(p => p.Id == id);

        return MapToPollDto(poll);
    }

    private PollDto MapToPollDto(Poll poll)
    {
        var totalVotes = poll.Votes.Count;
        return new PollDto
        {
            Id = poll.Id,
            Question = poll.Question,
            Status = poll.Status.ToString(),
            CreatedByName = poll.CreatedBy.FirstName + " " + poll.CreatedBy.LastName,
            CreatedOn = poll.CreatedOn,
            ClosedOn = poll.ClosedOn,
            Options = poll.Options.OrderBy(o => o.SortOrder).Select(o => new PollOptionDto
            {
                Id = o.Id,
                Label = o.Label,
                SortOrder = o.SortOrder
            }).ToList(),
            Results = poll.Options.Select(o => new PollResultDto
            {
                OptionId = o.Id,
                Label = o.Label,
                VoteCount = o.Votes.Count,
                Percentage = totalVotes > 0 ? (decimal)o.Votes.Count / totalVotes * 100 : 0
            }).ToList()
        };
    }
}
