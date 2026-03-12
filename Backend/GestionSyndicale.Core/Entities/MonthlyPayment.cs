namespace GestionSyndicale.Core.Entities;

/// <summary>
/// Représente un paiement mensuel de cotisation pour un appartement
/// </summary>
public class MonthlyPayment
{
    public int Id { get; set; }
    public int ApartmentId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; } // 1-12
    public decimal Amount { get; set; } // Montant de la cotisation mensuelle
    public DateTime PaymentDate { get; set; }
    public string? ReferenceNumber { get; set; }
    public int RecordedById { get; set; } // User qui a enregistré le paiement
    public DateTime RecordedAt { get; set; }
    public string? Notes { get; set; }
    public bool IsPaid { get; set; } = true; // False si le paiement est annulé/décoché

    // Navigation properties
    public Apartment Apartment { get; set; } = null!;
    public User RecordedBy { get; set; } = null!;
}
