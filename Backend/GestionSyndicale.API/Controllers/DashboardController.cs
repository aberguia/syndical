using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionSyndicale.Core.DTOs.Dashboard;
using GestionSyndicale.Core.Entities;
using GestionSyndicale.Infrastructure.Data;
using System.Security.Claims;

namespace GestionSyndicale.API.Controllers;

[Authorize]
[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public DashboardController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetDashboard()
    {
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        if (userRole == "SuperAdmin" || userRole == "Admin")
        {
            return Ok(await GetAdminDashboard());
        }
        else
        {
            return Ok(await GetAdherentDashboard(userId));
        }
    }

    private async Task<AdminDashboardDto> GetAdminDashboard()
    {
        var currentYear = DateTime.UtcNow.Year;
        var startOfYear = new DateTime(currentYear, 1, 1);
        var endOfYear = new DateTime(currentYear, 12, 31);
        var monthlyAmount = _configuration.GetValue<decimal>("Finance:MonthlyContributionAmount", 100);

        var dashboard = new AdminDashboardDto
        {
            Kpis = new KpisDto
            {
                BuildingsCount = await _context.Buildings.CountAsync(),
                ApartmentsCount = await _context.Apartments.CountAsync(),
                AdherentsCount = await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).CountAsync(u => u.UserRoles.Any(ur => ur.Role.Name == "Adherent")),
                TotalRevenuesCurrentYear = await _context.MonthlyPayments.Where(mp => mp.IsPaid && mp.PaymentDate >= startOfYear && mp.PaymentDate <= endOfYear).CountAsync() * monthlyAmount +
                                          await _context.OtherRevenues.Where(or => or.RevenueDate >= startOfYear && or.RevenueDate <= endOfYear).SumAsync(or => or.Amount),
                TotalExpensesCurrentYear = await _context.Expenses.Where(e => e.ExpenseDate >= startOfYear && e.ExpenseDate <= endOfYear).SumAsync(e => e.Amount)
            },
            RecentAnnouncements = await _context.Announcements.OrderByDescending(a => a.CreatedOn).Take(5).Select(a => new RecentAnnouncementDto
            {
                Id = a.Id,
                Title = a.Title,
                Status = a.Status.ToString(),
                CreatedOn = a.CreatedOn
            }).ToListAsync(),
            RecentPolls = await _context.Polls.OrderByDescending(p => p.CreatedOn).Take(5).Include(p => p.Votes).Select(p => new RecentPollDto
            {
                Id = p.Id,
                Question = p.Question,
                Status = p.Status.ToString(),
                TotalVotes = p.Votes.Count,
                CreatedOn = p.CreatedOn
            }).ToListAsync(),
            RecentExpenses = await _context.Expenses.OrderByDescending(e => e.ExpenseDate).Take(5).Select(e => new RecentExpenseDto
            {
                Id = e.Id,
                Description = e.Description,
                Amount = e.Amount,
                ExpenseDate = e.ExpenseDate
            }).ToListAsync(),
            RecentRevenues = await _context.OtherRevenues.OrderByDescending(r => r.RevenueDate).Take(5).Select(r => new RecentRevenueDto
            {
                Type = "Autre revenu",
                Description = r.Title,
                Amount = r.Amount,
                Date = r.RevenueDate
            }).ToListAsync(),
            ParkingLive = await GetParkingLive()
        };

        return dashboard;
    }

    private async Task<AdherentDashboardDto> GetAdherentDashboard(int userId)
    {
        var user = await _context.Users.Include(u => u.Apartment).ThenInclude(a => a!.Building).FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.Apartment == null) return new AdherentDashboardDto { Message = "Aucun appartement associé" };

        var apartmentId = user.Apartment.Id;
        var buildingId = user.Apartment.BuildingId;
        var currentYear = DateTime.UtcNow.Year;
        var monthlyAmount = _configuration.GetValue<decimal>("Finance:MonthlyContributionAmount", 100);

        // Calculate apartment contribution
        var paidMonthsCount = await _context.MonthlyPayments.CountAsync(mp => mp.ApartmentId == apartmentId && mp.Year == currentYear && mp.IsPaid);
        var annualAmount = monthlyAmount * 12;
        var paidAmount = paidMonthsCount * monthlyAmount;
        var remainingAmount = annualAmount - paidAmount;
        var isUpToDate = remainingAmount <= 0;

        var contribution = new ApartmentContributionDto
        {
            ApartmentId = apartmentId,
            ApartmentNumber = user.Apartment.ApartmentNumber,
            BuildingNumber = user.Apartment.Building.BuildingNumber,
            AnnualAmount = annualAmount,
            PaidAmount = paidAmount,
            RemainingAmount = remainingAmount,
            Status = isUpToDate ? "À jour" : "En retard",
            IsUpToDate = isUpToDate
        };

        // Buildings ranking
        var buildings = await _context.Buildings.Include(b => b.Apartments).ToListAsync();
        var buildingsData = new List<(int BuildingId, string BuildingNumber, decimal Percentage)>();

        foreach (var building in buildings)
        {
            var apartmentIds = building.Apartments.Select(a => a.Id).ToList();
            var totalExpected = apartmentIds.Count * 12 * monthlyAmount;
            var totalPaid = await _context.MonthlyPayments
                .Where(mp => apartmentIds.Contains(mp.ApartmentId) && mp.Year == currentYear && mp.IsPaid)
                .CountAsync() * monthlyAmount;

            var percentage = totalExpected > 0 ? (totalPaid / totalExpected * 100) : 0;
            buildingsData.Add((building.Id, building.BuildingNumber, percentage));
        }

        var ranking = buildingsData
            .OrderByDescending(b => b.Percentage)
            .Select((b, index) => new BuildingRankingDto
            {
                BuildingId = b.BuildingId,
                BuildingNumber = b.BuildingNumber,
                ContributionPercentage = b.Percentage,
                Rank = index + 1,
                IsMyBuilding = b.BuildingId == buildingId
            })
            .ToList();

        // Generate message
        var myRank = ranking.FirstOrDefault(r => r.IsMyBuilding)?.Rank ?? 0;
        var message = myRank switch
        {
            1 => "🎉 Vous êtes les premiers, félicitations !",
            <= 3 => "💪 Vous êtes dans le top 3 ! Continuez ainsi !",
            _ when myRank == ranking.Count => "⚠️ Vous êtes les derniers… pensez à faire votre cotisation pour faire monter votre immeuble.",
            _ => "💪 Vous pouvez faire mieux !"
        };

        if (!isUpToDate)
        {
            message = $"⚠️ Vous avez {remainingAmount:F2} MAD de reste à payer. " + message;
        }

        return new AdherentDashboardDto
        {
            Contribution = contribution,
            BuildingsRanking = ranking.Take(10).ToList(),
            ParkingLive = await GetParkingLive(),
            Message = message
        };
    }

    private async Task<ParkingLiveDto> GetParkingLive()
    {
        var totalPlaces = _configuration.GetValue<int>("Parking:TotalPlaces", 120);
        var parkingStatus = await _context.ParkingStatuses.OrderByDescending(ps => ps.UpdatedAt).FirstOrDefaultAsync();
        var occupiedPlaces = parkingStatus?.CurrentCars ?? 0;

        return new ParkingLiveDto
        {
            TotalPlaces = totalPlaces,
            OccupiedPlaces = occupiedPlaces,
            AvailablePlaces = totalPlaces - occupiedPlaces
        };
    }

    [HttpGet("building-ranking")]
    public async Task<ActionResult<BuildingRankingWidgetDto>> GetBuildingRanking()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users
            .Include(u => u.Apartment)
            .ThenInclude(a => a!.Building)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user?.Apartment == null)
        {
            return BadRequest(new { message = "Aucun appartement associé à cet utilisateur" });
        }

        var myBuildingId = user.Apartment.BuildingId;
        var currentYear = DateTime.UtcNow.Year;
        var monthlyAmount = _configuration.GetValue<decimal>("Finance:MonthlyContributionAmount", 100);

        // Check my payment status
        var myApartmentId = user.Apartment.Id;
        var paidMonthsCount = await _context.MonthlyPayments
            .CountAsync(mp => mp.ApartmentId == myApartmentId && mp.Year == currentYear && mp.IsPaid);
        var remainingAmount = (12 - paidMonthsCount) * monthlyAmount;
        var myPaymentStatus = remainingAmount <= 0 ? "UpToDate" : "Late";

        // Calculate all buildings ranking
        var buildings = await _context.Buildings
            .Include(b => b.Apartments)
            .ToListAsync();

        var buildingsData = new List<BuildingRankingItemDto>();

        foreach (var building in buildings)
        {
            var apartmentIds = building.Apartments.Select(a => a.Id).ToList();
            var expectedAmount = apartmentIds.Count * 12 * monthlyAmount;
            var paidAmount = await _context.MonthlyPayments
                .Where(mp => apartmentIds.Contains(mp.ApartmentId) && mp.Year == currentYear && mp.IsPaid)
                .CountAsync() * monthlyAmount;

            var collectionRate = expectedAmount > 0 ? Math.Round((paidAmount / expectedAmount) * 100, 2) : 0;

            buildingsData.Add(new BuildingRankingItemDto
            {
                BuildingId = building.Id,
                BuildingName = $"Immeuble {building.BuildingNumber}",
                PaidAmount = paidAmount,
                ExpectedAmount = expectedAmount,
                CollectionRate = collectionRate,
                Rank = 0 // Will be set after sorting
            });
        }

        // Sort by collection rate descending and assign ranks
        var sortedBuildings = buildingsData
            .OrderByDescending(b => b.CollectionRate)
            .ThenBy(b => b.BuildingName)
            .Select((b, index) =>
            {
                b.Rank = index + 1;
                return b;
            })
            .ToList();

        // Find my building rank and rate
        var myBuilding = sortedBuildings.FirstOrDefault(b => b.BuildingId == myBuildingId);
        var myBuildingRank = myBuilding?.Rank ?? 0;
        var myBuildingRate = myBuilding?.CollectionRate ?? 0;

        return Ok(new BuildingRankingWidgetDto
        {
            MyBuildingId = myBuildingId,
            MyBuildingRank = myBuildingRank,
            MyBuildingRate = myBuildingRate,
            MyPaymentStatus = myPaymentStatus,
            Items = sortedBuildings
        });
    }
}
