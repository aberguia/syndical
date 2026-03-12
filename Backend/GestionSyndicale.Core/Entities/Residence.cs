namespace GestionSyndicale.Core.Entities;

/// <summary>
/// Entité représentant la résidence gérée par cette instance
/// Une seule résidence par déploiement
/// </summary>
public class Residence
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Building> Buildings { get; set; } = new List<Building>();
}
