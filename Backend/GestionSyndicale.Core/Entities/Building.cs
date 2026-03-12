namespace GestionSyndicale.Core.Entities;

/// <summary>
/// Représente un immeuble dans la résidence
/// </summary>
public class Building
{
    public int Id { get; set; }
    public int ResidenceId { get; set; }
    public string BuildingNumber { get; set; } = string.Empty;
    public string? Name { get; set; }
    public int FloorCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public Residence Residence { get; set; } = null!;
    public ICollection<Apartment> Apartments { get; set; } = new List<Apartment>();
}
