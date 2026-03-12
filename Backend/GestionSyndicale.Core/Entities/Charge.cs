namespace GestionSyndicale.Core.Entities;

/// <summary>
/// Type de charge (mensuelle, annuelle, exceptionnelle)
/// </summary>
public class Charge
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; } // Montant total pour la résidence
    public string ChargeType { get; set; } = string.Empty; // Monthly, Annual, Exceptional
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedByUserId { get; set; }

    // Navigation
    public User CreatedBy { get; set; } = null!;
    public ICollection<CallForFunds> CallsForFunds { get; set; } = new List<CallForFunds>();
}
