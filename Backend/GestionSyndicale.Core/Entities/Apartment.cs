namespace GestionSyndicale.Core.Entities;

/// <summary>
/// Représente un appartement/lot dans un immeuble
/// Un appartement ne peut avoir qu'un seul adhérent principal
/// </summary>
public class Apartment
{
    public int Id { get; set; }
    public int BuildingId { get; set; }
    public string ApartmentNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public decimal Surface { get; set; } // En m²
    public int SharesCount { get; set; } // Tantièmes pour calcul charges
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false; // Soft delete

    // Navigation
    public Building Building { get; set; } = null!;
    public User? PrimaryOwner { get; set; } // Un seul propriétaire principal
    public ICollection<ApartmentComment> Comments { get; set; } = new List<ApartmentComment>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
