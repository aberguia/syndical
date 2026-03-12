namespace GestionSyndicale.Core.Entities;

/// <summary>
/// Paiement effectué par un adhérent
/// Historique immuable
/// </summary>
public class Payment
{
    public int Id { get; set; }
    public int ApartmentId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // Cash, Check, Transfer, Card
    public string? ReferenceNumber { get; set; } // Numéro de chèque ou référence virement
    public DateTime PaymentDate { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public int RecordedByUserId { get; set; }
    public string? Notes { get; set; }
    public string? ReceiptFilePath { get; set; } // Chemin du PDF généré

    // Navigation
    public Apartment Apartment { get; set; } = null!;
    public User RecordedBy { get; set; } = null!;
    public ICollection<PaymentAllocation> PaymentAllocations { get; set; } = new List<PaymentAllocation>();
}
