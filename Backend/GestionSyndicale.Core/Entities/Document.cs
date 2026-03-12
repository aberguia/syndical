namespace GestionSyndicale.Core.Entities;

/// <summary>
/// Documents partagés accessibles par tous les adhérents
/// Règlement intérieur, PV assemblées, etc.
/// </summary>
public class Document
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Category { get; set; } = string.Empty; // Règlement, PV, Technique, OtherRevenue, etc.
    public int? RelatedEntityId { get; set; } // ID de l'entité liée (OtherRevenue, etc.)
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public int UploadedByUserId { get; set; }
    public bool IsPublic { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public User UploadedBy { get; set; } = null!;
}
