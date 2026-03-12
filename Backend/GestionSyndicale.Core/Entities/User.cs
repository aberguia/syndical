namespace GestionSyndicale.Core.Entities;

/// <summary>
/// Utilisateur de l'application (Super Admin, Admin ou Adhérent)
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    
    public int? ApartmentId { get; set; } // Null pour Super Admin et Admin
    
    public bool IsEmailConfirmed { get; set; } = false;
    public bool IsActive { get; set; } = false; // Activé après validation OTP
    public bool IsDeleted { get; set; } = false; // Soft delete
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Navigation
    public Apartment? Apartment { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<OtpCode> OtpCodes { get; set; } = new List<OtpCode>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<ApartmentComment> ApartmentComments { get; set; } = new List<ApartmentComment>();
}
