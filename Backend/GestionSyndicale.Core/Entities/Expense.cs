namespace GestionSyndicale.Core.Entities;

/// <summary>
/// Dépense effectuée pour la résidence
/// Avec pièces justificatives obligatoires
/// </summary>
public class Expense
{
    public int Id { get; set; }
    public int? SupplierId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public int RecordedByUserId { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? Notes { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public Supplier? Supplier { get; set; }
    public User RecordedBy { get; set; } = null!;
    public ICollection<ExpenseAttachment> Attachments { get; set; } = new List<ExpenseAttachment>();
}
