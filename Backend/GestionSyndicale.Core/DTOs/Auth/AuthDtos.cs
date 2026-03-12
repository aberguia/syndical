namespace GestionSyndicale.Core.DTOs.Auth;

/// <summary>
/// DTO pour l'inscription d'un nouvel adhérent
/// </summary>
public class RegisterRequestDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string BuildingNumber { get; set; } = string.Empty;
    public string ApartmentNumber { get; set; } = string.Empty;
}

/// <summary>
/// DTO pour la validation du code OTP
/// </summary>
public class ValidateOtpRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
}

/// <summary>
/// DTO pour la réinitialisation du mot de passe
/// </summary>
public class ResetPasswordRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// DTO pour la connexion
/// </summary>
public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// DTO de réponse après connexion
/// </summary>
public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public int? ApartmentId { get; set; }
    public DateTime ExpiresAt { get; set; }
}
