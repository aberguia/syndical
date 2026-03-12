namespace GestionSyndicale.Core.Entities;

/// <summary>
/// Fournisseur pour traçabilité des dépenses
/// </summary>
public class Supplier
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ServiceCategory { get; set; } = string.Empty; // Plomberie, Électricité, etc.
    public string? Description { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public int? UpdatedByUserId { get; set; }

    // Navigation
    public User? CreatedByUser { get; set; }
    public User? UpdatedByUser { get; set; }
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
