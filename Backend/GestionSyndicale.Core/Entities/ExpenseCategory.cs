namespace GestionSyndicale.Core.Entities;

/// <summary>
/// Catégorie de dépense pour classification
/// </summary>
public class ExpenseCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // Entretien, Réparation, Assurance, etc.
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
