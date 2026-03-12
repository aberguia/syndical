namespace GestionSyndicale.Core.Entities;

/// <summary>
/// Autres revenus de la résidence (hors cotisations mensuelles)
/// Ex: donations, revenus exceptionnels, intérêts bancaires, etc.
/// </summary>
public class OtherRevenue
{
    public int Id { get; set; }
    public DateTime RevenueDate { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public int RecordedByUserId { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public User RecordedBy { get; set; } = null!;
    // Note: Les documents sont liés via Category="OtherRevenue" et RelatedEntityId
}
