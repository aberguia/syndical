using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionSyndicale.Core.DTOs.Polls;
using GestionSyndicale.Core.Entities;
using GestionSyndicale.Infrastructure.Data;
using System.Security.Claims;

namespace GestionSyndicale.API.Controllers;

[Authorize]
[ApiController]
[Route("api/portal/polls")]
public class PortalPollsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PortalPollsController> _logger;

    public PortalPollsController(ApplicationDbContext context, ILogger<PortalPollsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<PortalPollDto>>> GetPublished()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var polls = await _context.Polls
            .Where(p => p.Status == PollStatus.Published || p.Status == PollStatus.Closed)
            .Include(p => p.Options)
            .ThenInclude(o => o.Votes)
            .Include(p => p.Votes)
            .OrderByDescending(p => p.CreatedOn)
            .ToListAsync();

        var result = polls.Select(p =>
        {
            var myVote = p.Votes.FirstOrDefault(v => v.AdherentId == userId);
            var totalVotes = p.Votes.Count;

            return new PortalPollDto
            {
                Id = p.Id,
                Question = p.Question,
                Status = p.Status.ToString(),
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
                    Percentage = totalVotes > 0 ? (decimal)o.Votes.Count / totalVotes * 100 : 0
                }).ToList(),
                HasVoted = myVote != null,
                MyVoteOptionId = myVote?.PollOptionId,
                TotalVotes = totalVotes
            };
        }).ToList();

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PortalPollDto>> GetById(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var poll = await _context.Polls
            .Where(p => p.Id == id && (p.Status == PollStatus.Published || p.Status == PollStatus.Closed))
            .Include(p => p.Options)
            .ThenInclude(o => o.Votes)
            .Include(p => p.Votes)
            .FirstOrDefaultAsync();

        if (poll == null) return NotFound();

        var myVote = poll.Votes.FirstOrDefault(v => v.AdherentId == userId);
        var totalVotes = poll.Votes.Count;

        return Ok(new PortalPollDto
        {
            Id = poll.Id,
            Question = poll.Question,
            Status = poll.Status.ToString(),
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
            }).ToList(),
            HasVoted = myVote != null,
            MyVoteOptionId = myVote?.PollOptionId,
            TotalVotes = totalVotes
        });
    }

    [HttpPost("{id}/vote")]
    public async Task<IActionResult> Vote(int id, [FromBody] PollVoteDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var poll = await _context.Polls
            .Include(p => p.Votes.Where(v => v.AdherentId == userId))
            .FirstOrDefaultAsync(p => p.Id == id);

        if (poll == null) return NotFound("Poll not found");
        if (poll.Status == PollStatus.Closed) return BadRequest("Poll is closed");
        if (poll.Status != PollStatus.Published) return BadRequest("Poll is not published");

        // Verify option exists
        var optionExists = await _context.PollOptions.AnyAsync(o => o.Id == dto.PollOptionId && o.PollId == id);
        if (!optionExists) return BadRequest("Invalid option");

        var existingVote = poll.Votes.FirstOrDefault(v => v.AdherentId == userId);

        if (existingVote != null)
        {
            // Update vote
            existingVote.PollOptionId = dto.PollOptionId;
            existingVote.VotedOn = DateTime.UtcNow;
        }
        else
        {
            // Create new vote
            _context.PollVotes.Add(new PollVote
            {
                PollId = id,
                PollOptionId = dto.PollOptionId,
                AdherentId = userId,
                VotedOn = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Vote recorded successfully" });
    }
}
