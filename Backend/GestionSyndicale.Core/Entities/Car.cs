namespace GestionSyndicale.Core.Entities;

/// <summary>
/// Représente une voiture dans le parking
/// </summary>
public class Car
{
    public int Id { get; set; }
    public CarBrand Brand { get; set; }
    public int PlatePart1 { get; set; }
    public string PlatePart2 { get; set; } = string.Empty;
    public int PlatePart3 { get; set; }
    public CarType CarType { get; set; }
    public int MemberId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Navigation
    public User Member { get; set; } = null!;
}

/// <summary>
/// Type de voiture
/// </summary>
public enum CarType
{
    Primary = 0,   // Voiture principale (propriétaire)
    Tenant = 1,    // Voiture locataire
    Visitor = 2    // Voiture visiteur
}
