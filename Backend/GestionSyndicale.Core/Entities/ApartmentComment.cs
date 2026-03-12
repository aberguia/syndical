namespace GestionSyndicale.Core.Entities;

/// <summary>
/// Commentaire interne sur un appartement (visible uniquement par Super Admin et Admin)
/// </summary>
public class ApartmentComment
{
    public int Id { get; set; }
    public int ApartmentId { get; set; }
    public int CreatedByUserId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Navigation
    public Apartment Apartment { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;
}
