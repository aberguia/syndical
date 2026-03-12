namespace GestionSyndicale.Core.Entities;

/// <summary>
/// Pièce jointe d'une news (image, PDF)
/// </summary>
public class NewsAttachment
{
    public int Id { get; set; }
    public int NewsPostId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public NewsPost NewsPost { get; set; } = null!;
}
