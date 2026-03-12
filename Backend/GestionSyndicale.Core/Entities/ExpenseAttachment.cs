namespace GestionSyndicale.Core.Entities;

/// <summary>
/// Pièce justificative d'une dépense (facture, devis, etc.)
/// Stockage en local avec metadata
/// </summary>
public class ExpenseAttachment
{
    public int Id { get; set; }
    public int ExpenseId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty; // image/jpeg, application/pdf
    public long FileSize { get; set; } // En bytes
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public int UploadedByUserId { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public Expense Expense { get; set; } = null!;
    public User UploadedBy { get; set; } = null!;
}
