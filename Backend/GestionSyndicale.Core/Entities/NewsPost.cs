namespace GestionSyndicale.Core.Entities;

/// <summary>
/// Publication de news/affichage pour tous les résidents
/// </summary>
public class NewsPost
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    public int PublishedByUserId { get; set; }
    public bool IsPublished { get; set; } = true;
    public DateTime? UpdatedAt { get; set; }
    public string? Category { get; set; } // Général, Travaux, Réunion, etc.

    // Navigation
    public User PublishedBy { get; set; } = null!;
    public ICollection<NewsAttachment> Attachments { get; set; } = new List<NewsAttachment>();
}
