namespace GestionSyndicale.Core.Entities;

/// <summary>
/// Rapport mensuel généré automatiquement
/// Contient synthèse financière du mois
/// </summary>
public class MonthlyReport
{
    public int Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalPaymentsReceived { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal Balance { get; set; }
    public int PaymentsCount { get; set; }
    public int ExpensesCount { get; set; }
    public string? ReportFilePath { get; set; } // PDF généré
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public int GeneratedByUserId { get; set; }

    // Navigation
    public User GeneratedBy { get; set; } = null!;
}
