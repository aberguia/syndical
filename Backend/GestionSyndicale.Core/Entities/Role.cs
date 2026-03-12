namespace GestionSyndicale.Core.Entities;

/// <summary>
/// Rôle utilisateur: SuperAdmin, Admin, Adherent
/// </summary>
public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // SuperAdmin, Admin, Adherent
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

/// <summary>
/// Table de jointure entre User et Role
/// </summary>
public class UserRole
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
