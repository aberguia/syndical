namespace GestionSyndicale.Core.DTOs.Dashboard;

// ========================================
// NOUVEAUX DTOs pour Dashboard role-based
// ========================================

public class AdminDashboardDto
{
    public KpisDto Kpis { get; set; } = new();
    public List<RecentAnnouncementDto> RecentAnnouncements { get; set; } = new();
    public List<RecentPollDto> RecentPolls { get; set; } = new();
    public List<RecentExpenseDto> RecentExpenses { get; set; } = new();
    public List<RecentRevenueDto> RecentRevenues { get; set; } = new();
    public ParkingLiveDto ParkingLive { get; set; } = new();
}

public class KpisDto
{
    public int BuildingsCount { get; set; }
    public int ApartmentsCount { get; set; }
    public int AdherentsCount { get; set; }
    public decimal TotalRevenuesCurrentYear { get; set; }
    public decimal TotalExpensesCurrentYear { get; set; }
}

public class RecentAnnouncementDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
}

public class RecentPollDto
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TotalVotes { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class RecentExpenseDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
}

public class RecentRevenueDto
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}

public class ParkingLiveDto
{
    public int TotalPlaces { get; set; }
    public int OccupiedPlaces { get; set; }
    public int AvailablePlaces { get; set; }
}

public class AdherentDashboardDto
{
    public ApartmentContributionDto Contribution { get; set; } = new();
    public List<BuildingRankingDto> BuildingsRanking { get; set; } = new();
    public ParkingLiveDto ParkingLive { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}

public class ApartmentContributionDto
{
    public int ApartmentId { get; set; }
    public string ApartmentNumber { get; set; } = string.Empty;
    public string BuildingNumber { get; set; } = string.Empty;
    public decimal AnnualAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string Status { get; set; } = string.Empty; // "À jour" or "En retard"
    public bool IsUpToDate { get; set; }
}

public class BuildingRankingDto
{
    public int BuildingId { get; set; }
    public string BuildingNumber { get; set; } = string.Empty;
    public decimal ContributionPercentage { get; set; }
    public int Rank { get; set; }
    public bool IsMyBuilding { get; set; }
}

// ========================================
// DTOs pour le widget de classement détaillé
// ========================================

public class BuildingRankingWidgetDto
{
    public int MyBuildingId { get; set; }
    public int MyBuildingRank { get; set; }
    public decimal MyBuildingRate { get; set; }
    public string MyPaymentStatus { get; set; } = string.Empty; // "UpToDate" | "Late"
    public List<BuildingRankingItemDto> Items { get; set; } = new();
}

public class BuildingRankingItemDto
{
    public int BuildingId { get; set; }
    public string BuildingName { get; set; } = string.Empty;
    public decimal PaidAmount { get; set; }
    public decimal ExpectedAmount { get; set; }
    public decimal CollectionRate { get; set; } // 0..100
    public int Rank { get; set; }
}

// ========================================
// ANCIENS DTOs (conserver pour compatibilité)
// ========================================

/// <summary>
/// DTO pour le dashboard du Syndic/Admin
/// </summary>
public class DashboardStatsDto
{
    public ResidenceStatsDto Residence { get; set; } = new();
    public FinancialStatsDto Financial { get; set; } = new();
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
}

public class ResidenceStatsDto
{
    public int TotalBuildings { get; set; }
    public int TotalApartments { get; set; }
    public int ActiveMembers { get; set; }
    public int PendingRegistrations { get; set; }
}

public class FinancialStatsDto
{
    public decimal CurrentMonthPayments { get; set; }
    public decimal CurrentMonthExpenses { get; set; }
    public decimal CurrentMonthBalance { get; set; }
    public decimal TotalOutstanding { get; set; }
    public int OverdueCount { get; set; }
}

public class RecentActivityDto
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? UserName { get; set; }
}

/// <summary>
/// DTO pour le dashboard de l'adhérent
/// </summary>
public class MemberDashboardDto
{
    public ApartmentInfoDto Apartment { get; set; } = new();
    public ApartmentBalanceDto Balance { get; set; } = new();
    public List<NotificationDto> RecentNotifications { get; set; } = new();
    public List<NewsPostSummaryDto> RecentNews { get; set; } = new();
}

public class ApartmentInfoDto
{
    public int Id { get; set; }
    public string BuildingNumber { get; set; } = string.Empty;
    public string ApartmentNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public decimal Surface { get; set; }
    public int SharesCount { get; set; }
}

public class ApartmentBalanceDto
{
    public decimal TotalDue { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal Balance { get; set; }
    public int PendingCallsCount { get; set; }
    public int OverdueCallsCount { get; set; }
}

public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class NewsPostSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ContentPreview { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public string? Category { get; set; }
}
