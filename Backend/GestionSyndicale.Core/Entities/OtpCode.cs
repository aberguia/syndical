namespace GestionSyndicale.Core.Entities;

/// <summary>
/// Code OTP pour validation d'inscription
/// Expire après 15 minutes
/// </summary>
public class OtpCode
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Code { get; set; } = string.Empty; // 6 chiffres
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime? UsedAt { get; set; }
    public string Purpose { get; set; } = string.Empty; // Registration, PasswordReset

    // Navigation
    public User User { get; set; } = null!;
}
