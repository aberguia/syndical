namespace GestionSyndicale.Core.Entities;

/// <summary>
/// Appel de fonds pour un appartement spécifique
/// Calculé selon les tantièmes de l'appartement
/// </summary>
public class CallForFunds
{
    public int Id { get; set; }
    public int ChargeId { get; set; }
    public int ApartmentId { get; set; }
    public decimal AmountDue { get; set; } // Montant calculé selon tantièmes
    public decimal AmountPaid { get; set; } = 0;
    public decimal AmountRemaining { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, PartiallyPaid, Paid, Overdue
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Charge Charge { get; set; } = null!;
    public Apartment Apartment { get; set; } = null!;
    public ICollection<PaymentAllocation> PaymentAllocations { get; set; } = new List<PaymentAllocation>();
}
