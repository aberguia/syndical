using GestionSyndicale.Core.DTOs.Auth;
using GestionSyndicale.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionSyndicale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IAuditService _auditService;

    public AuthController(IAuthService authService, IAuditService auditService)
    {
        _authService = authService;
        _auditService = auditService;
    }

    /// <summary>
    /// Inscription d'un nouvel adhérent
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var (success, message) = await _authService.RegisterAsync(request);

        if (!success)
        {
            return BadRequest(new { message });
        }

        await _auditService.LogAsync(
            null,
            "Register",
            "User",
            null,
            null,
            $"Email: {request.Email}",
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
        );

        return Ok(new { message });
    }

    /// <summary>
    /// Validation du code OTP
    /// </summary>
    [HttpPost("validate-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateOtp([FromBody] ValidateOtpRequestDto request)
    {
        var (success, message) = await _authService.ValidateOtpAsync(request);

        if (!success)
        {
            return BadRequest(new { message });
        }

        await _auditService.LogAsync(
            null,
            "ValidateOTP",
            "User",
            null,
            null,
            $"Email: {request.Email}",
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
        );

        return Ok(new { message });
    }

    /// <summary>
    /// Connexion
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var response = await _authService.LoginAsync(request);

        if (response == null)
        {
            await _auditService.LogAsync(
                null,
                "LoginFailed",
                "User",
                null,
                null,
                $"Email: {request.Email}",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
            );

            return Unauthorized(new { message = "Email ou mot de passe incorrect, ou compte non activé." });
        }

        await _auditService.LogAsync(
            null,
            "Login",
            "User",
            null,
            null,
            $"Email: {request.Email}",
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
        );

        return Ok(response);
    }

    /// <summary>
    /// Renvoyer un code OTP pour l'inscription
    /// </summary>
    [HttpPost("resend-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendOtp([FromQuery] string email)
    {
        var success = await _authService.ResendOtpAsync(email);

        if (!success)
        {
            return BadRequest(new { message = "Impossible de renvoyer le code. Vérifiez l'email ou le compte est déjà activé." });
        }

        return Ok(new { message = "Un nouveau code a été envoyé à votre email." });
    }

    /// <summary>
    /// Envoyer un code OTP pour la réinitialisation du mot de passe
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromQuery] string email)
    {
        var success = await _authService.SendPasswordResetOtpAsync(email);

        if (!success)
        {
            return BadRequest(new { message = "Aucun compte actif trouvé avec cet email." });
        }

        return Ok(new { message = "Un code de réinitialisation a été envoyé à votre email." });
    }

    /// <summary>
    /// Réinitialiser le mot de passe avec le code OTP
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        var (success, message) = await _authService.ResetPasswordAsync(request);

        if (!success)
        {
            return BadRequest(new { message });
        }

        await _auditService.LogAsync(
            null,
            "PasswordReset",
            "User",
            null,
            null,
            $"Email: {request.Email}",
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
        );

        return Ok(new { message });
    }
}
