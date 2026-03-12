using GestionSyndicale.Core.DTOs.Auth;

namespace GestionSyndicale.Core.Interfaces;

/// <summary>
/// Service d'authentification avec OTP
/// </summary>
public interface IAuthService
{
    Task<(bool Success, string Message)> RegisterAsync(RegisterRequestDto request);
    Task<(bool Success, string Message)> ValidateOtpAsync(ValidateOtpRequestDto request);
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
    Task<bool> ResendOtpAsync(string email);
    Task<bool> SendPasswordResetOtpAsync(string email);
    Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequestDto request);
    Task<string> GenerateJwtTokenAsync(int userId);
}
