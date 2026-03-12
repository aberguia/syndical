namespace GestionSyndicale.Core.Entities;

/// <summary>
/// Notification interne pour les utilisateurs
/// Affichée dans l'interface, pas de service externe
/// </summary>
public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Info, Warning, Payment, News
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
    public string? RelatedEntityType { get; set; } // Payment, NewsPost, etc.
    public int? RelatedEntityId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
