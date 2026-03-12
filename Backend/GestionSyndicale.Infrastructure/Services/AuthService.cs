using GestionSyndicale.Core.DTOs.Auth;
using GestionSyndicale.Core.Entities;
using GestionSyndicale.Core.Interfaces;
using GestionSyndicale.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GestionSyndicale.Infrastructure.Services;

/// <summary>
/// Service d'authentification avec OTP
/// </summary>
public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IEmailService emailService, IConfiguration configuration)
    {
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequestDto request)
    {
        // Vérifier si l'email existe déjà
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return (false, "Cet email est déjà utilisé.");
        }

        // Vérifier que l'appartement existe
        var apartment = await _context.Apartments
            .Include(a => a.Building)
            .FirstOrDefaultAsync(a => 
                a.Building.BuildingNumber == request.BuildingNumber && 
                a.ApartmentNumber == request.ApartmentNumber &&
                a.IsActive);

        if (apartment == null)
        {
            return (false, "Appartement introuvable. Veuillez vérifier le numéro d'immeuble et d'appartement.");
        }

        // Vérifier que l'appartement n'a pas déjà un propriétaire
        if (await _context.Users.AnyAsync(u => u.ApartmentId == apartment.Id))
        {
            return (false, "Cet appartement a déjà un compte adhérent associé.");
        }

        // Créer l'utilisateur
        var user = new User
        {
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            ApartmentId = apartment.Id,
            IsEmailConfirmed = false,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Assigner le rôle Adherent
        var adherentRole = await _context.Roles.FirstAsync(r => r.Name == "Adherent");
        _context.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = adherentRole.Id
        });
        await _context.SaveChangesAsync();

        // Générer et envoyer le code OTP
        var otpCode = GenerateOtpCode();
        var otp = new OtpCode
        {
            UserId = user.Id,
            Code = otpCode,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            Purpose = "Registration"
        };

        _context.OtpCodes.Add(otp);
        await _context.SaveChangesAsync();

        // Envoyer l'email
        await _emailService.SendOtpEmailAsync(user.Email, otpCode, user.FirstName);

        return (true, "Inscription réussie. Un code de validation a été envoyé à votre email.");
    }

    public async Task<(bool Success, string Message)> ValidateOtpAsync(ValidateOtpRequestDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
        {
            return (false, "Utilisateur introuvable.");
        }

        var otp = await _context.OtpCodes
            .Where(o => o.UserId == user.Id && 
                       o.Code == request.OtpCode && 
                       !o.IsUsed && 
                       o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (otp == null)
        {
            return (false, "Code OTP invalide ou expiré.");
        }

        // Marquer l'OTP comme utilisé
        otp.IsUsed = true;
        otp.UsedAt = DateTime.UtcNow;

        // Activer l'utilisateur seulement si c'est une inscription
        if (otp.Purpose == "Registration")
        {
            user.IsEmailConfirmed = true;
            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Message différent selon le purpose
        var message = otp.Purpose == "Registration" 
            ? "Compte activé avec succès. Vous pouvez maintenant vous connecter."
            : "Code validé avec succès.";

        return (true, message);
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.Apartment)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        // DEBUG
        Console.WriteLine($"[DEBUG] User found: {user != null}");
        if (user != null)
        {
            var computedHash = HashPassword(request.Password);
            Console.WriteLine($"[DEBUG] Password from request: {request.Password}");
            Console.WriteLine($"[DEBUG] Hash in DB: {user.PasswordHash}");
            Console.WriteLine($"[DEBUG] Computed hash: {computedHash}");
            Console.WriteLine($"[DEBUG] Hashes match: {computedHash == user.PasswordHash}");
            Console.WriteLine($"[DEBUG] User IsActive: {user.IsActive}");
        }

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            return null;
        }

        if (!user.IsActive)
        {
            return null; // Compte non activé
        }

        // Mettre à jour la date de dernière connexion
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Générer le token JWT
        var token = await GenerateJwtTokenAsync(user.Id);

        return new LoginResponseDto
        {
            Token = token,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
            ApartmentId = user.ApartmentId,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
    }

    public async Task<bool> ResendOtpAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || user.IsActive)
        {
            return false;
        }

        // Invalider les anciens codes
        var oldCodes = await _context.OtpCodes
            .Where(o => o.UserId == user.Id && !o.IsUsed)
            .ToListAsync();

        foreach (var code in oldCodes)
        {
            code.IsUsed = true;
        }

        // Générer un nouveau code
        var otpCode = GenerateOtpCode();
        var otp = new OtpCode
        {
            UserId = user.Id,
            Code = otpCode,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            Purpose = "Registration"
        };

        _context.OtpCodes.Add(otp);
        await _context.SaveChangesAsync();

        // Envoyer l'email
        await _emailService.SendOtpEmailAsync(user.Email, otpCode, user.FirstName);

        return true;
    }

    public async Task<bool> SendPasswordResetOtpAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || !user.IsActive)
        {
            return false; // Le compte doit exister et être activé pour réinitialiser le mot de passe
        }

        // Invalider les anciens codes
        var oldCodes = await _context.OtpCodes
            .Where(o => o.UserId == user.Id && !o.IsUsed)
            .ToListAsync();

        foreach (var code in oldCodes)
        {
            code.IsUsed = true;
        }

        // Générer un nouveau code
        var otpCode = GenerateOtpCode();
        var otp = new OtpCode
        {
            UserId = user.Id,
            Code = otpCode,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            Purpose = "PasswordReset"
        };

        _context.OtpCodes.Add(otp);
        await _context.SaveChangesAsync();

        // Envoyer l'email
        await _emailService.SendOtpEmailAsync(user.Email, otpCode, user.FirstName);

        return true;
    }

    public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || !user.IsActive)
        {
            return (false, "Utilisateur introuvable ou compte non activé.");
        }

        // Vérifier le code OTP
        var otp = await _context.OtpCodes
            .Where(o => o.UserId == user.Id && 
                       o.Code == request.OtpCode && 
                       !o.IsUsed && 
                       o.ExpiresAt > DateTime.UtcNow &&
                       o.Purpose == "PasswordReset")
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (otp == null)
        {
            return (false, "Code OTP invalide ou expiré.");
        }

        // Marquer l'OTP comme utilisé
        otp.IsUsed = true;
        otp.UsedAt = DateTime.UtcNow;

        // Mettre à jour le mot de passe
        user.PasswordHash = HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (true, "Mot de passe réinitialisé avec succès. Vous pouvez maintenant vous connecter.");
    }

    public async Task<string> GenerateJwtTokenAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstAsync(u => u.Id == userId);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
        };

        // Ajouter les rôles comme claims
        foreach (var userRole in user.UserRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
        }

        if (user.ApartmentId.HasValue)
        {
            claims.Add(new Claim("ApartmentId", user.ApartmentId.Value.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateOtpCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return BitConverter.ToString(hashedBytes).Replace("-", "");
    }

    private bool VerifyPassword(string password, string hashedPassword)
    {
        return HashPassword(password) == hashedPassword;
    }
}
