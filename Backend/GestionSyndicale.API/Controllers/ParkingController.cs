using GestionSyndicale.Core.DTOs.Parking;
using GestionSyndicale.Core.Entities;
using GestionSyndicale.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using GestionSyndicale.API.Hubs;

namespace GestionSyndicale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ParkingController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IHubContext<ParkingHub> _hubContext;
    private readonly ILogger<ParkingController> _logger;
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public ParkingController(
        ApplicationDbContext context,
        IConfiguration configuration,
        IHubContext<ParkingHub> hubContext,
        ILogger<ParkingController> logger)
    {
        _context = context;
        _configuration = configuration;
        _hubContext = hubContext;
        _logger = logger;
    }

    private int TotalPlaces => _configuration.GetValue<int>("Parking:TotalPlaces", 120);

    [HttpGet("status")]
    public async Task<ActionResult<ParkingStatusDto>> GetStatus()
    {
        try
        {
            var status = await GetOrCreateParkingStatus();
            return Ok(BuildStatusDto(status));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting parking status");
            return StatusCode(500, new { message = "Error getting parking status" });
        }
    }

    [HttpPost("increment")]
    public async Task<ActionResult<ParkingStatusDto>> Increment([FromBody] IncrementDecrementDto? dto = null)
    {
        await _semaphore.WaitAsync();
        try
        {
            var count = dto?.Count ?? 1;
            if (count <= 0)
                return BadRequest(new { message = "Count must be positive" });

            var status = await GetOrCreateParkingStatus();
            status.CurrentCars += count;
            status.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var statusDto = BuildStatusDto(status);
            
            // Broadcast to all clients
            await _hubContext.Clients.Group("Parking").SendAsync("ParkingStatusUpdated", statusDto);

            return Ok(statusDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing parking count");
            return StatusCode(500, new { message = "Error incrementing parking count" });
        }
        finally
        {
            _semaphore.Release();
        }
    }

    [HttpPost("decrement")]
    public async Task<ActionResult<ParkingStatusDto>> Decrement([FromBody] IncrementDecrementDto? dto = null)
    {
        await _semaphore.WaitAsync();
        try
        {
            var count = dto?.Count ?? 1;
            if (count <= 0)
                return BadRequest(new { message = "Count must be positive" });

            var status = await GetOrCreateParkingStatus();
            status.CurrentCars = Math.Max(0, status.CurrentCars - count);
            status.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var statusDto = BuildStatusDto(status);
            
            // Broadcast to all clients
            await _hubContext.Clients.Group("Parking").SendAsync("ParkingStatusUpdated", statusDto);

            return Ok(statusDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrementing parking count");
            return StatusCode(500, new { message = "Error decrementing parking count" });
        }
        finally
        {
            _semaphore.Release();
        }
    }

    [HttpPut("status")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ParkingStatusDto>> SetCurrentCars([FromBody] SetCurrentCarsDto dto)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (dto.CurrentCars < 0)
                return BadRequest(new { message = "CurrentCars cannot be negative" });

            var status = await GetOrCreateParkingStatus();
            status.CurrentCars = dto.CurrentCars;
            status.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var statusDto = BuildStatusDto(status);
            
            // Broadcast to all clients
            await _hubContext.Clients.Group("Parking").SendAsync("ParkingStatusUpdated", statusDto);

            return Ok(statusDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting parking count");
            return StatusCode(500, new { message = "Error setting parking count" });
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<ParkingStatus> GetOrCreateParkingStatus()
    {
        var status = await _context.ParkingStatuses.FirstOrDefaultAsync();
        if (status == null)
        {
            status = new ParkingStatus
            {
                CurrentCars = 0,
                UpdatedAt = DateTime.UtcNow
            };
            _context.ParkingStatuses.Add(status);
            await _context.SaveChangesAsync();
        }
        return status;
    }

    private ParkingStatusDto BuildStatusDto(ParkingStatus status)
    {
        var totalPlaces = TotalPlaces;
        var availablePlaces = totalPlaces - status.CurrentCars;
        
        string statusText;
        if (status.CurrentCars > totalPlaces)
            statusText = "Dépassé";
        else if (status.CurrentCars == totalPlaces)
            statusText = "Plein";
        else
            statusText = "OK";

        return new ParkingStatusDto
        {
            TotalPlaces = totalPlaces,
            CurrentCars = status.CurrentCars,
            AvailablePlaces = availablePlaces,
            Status = statusText,
            UpdatedAt = status.UpdatedAt
        };
    }
}
