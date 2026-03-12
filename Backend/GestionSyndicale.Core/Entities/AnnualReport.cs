namespace GestionSyndicale.Core.Entities;

/// <summary>
/// Rapport annuel consolidé
/// </summary>
public class AnnualReport
{
    public int Id { get; set; }
    public int Year { get; set; }
    public decimal TotalPaymentsReceived { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal Balance { get; set; }
    public string? ReportFilePath { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public int GeneratedByUserId { get; set; }

    // Navigation
    public User GeneratedBy { get; set; } = null!;
}
