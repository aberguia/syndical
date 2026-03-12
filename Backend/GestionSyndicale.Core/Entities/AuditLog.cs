namespace GestionSyndicale.Core.Entities;

/// <summary>
/// Audit des actions sensibles effectuées dans l'application
/// Immuable pour traçabilité complète
/// </summary>
public class AuditLog
{
    public int Id { get; set; }
    public int? UserId { get; set; } // Nullable pour actions système
    public string Action { get; set; } = string.Empty; // Create, Update, Delete, Login, etc.
    public string EntityType { get; set; } = string.Empty; // Payment, User, Expense, etc.
    public int? EntityId { get; set; }
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public string IpAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User? User { get; set; }
}
