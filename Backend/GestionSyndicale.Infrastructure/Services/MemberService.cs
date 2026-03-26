using GestionSyndicale.Core.DTOs;
using GestionSyndicale.Core.Entities;
using GestionSyndicale.Core.Interfaces;
using GestionSyndicale.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace GestionSyndicale.Infrastructure.Services;

/// <summary>
/// Service de gestion des adhérents (membres/utilisateurs)
/// </summary>
public class MemberService : IMemberService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public MemberService(ApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<IEnumerable<MemberListDto>> GetAllMembersAsync()
    {
        var members = await _context.Users
            .Where(u => !u.IsDeleted)
            .Include(u => u.Apartment)
                .ThenInclude(a => a!.Building)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Select(u => new MemberListDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                PhoneNumber = u.Phone,
                Role = u.UserRoles.Any() ? u.UserRoles.First().Role.Name : "User",
                IsActive = u.IsActive,
                ApartmentId = u.ApartmentId,
                ApartmentNumber = u.Apartment != null ? u.Apartment.ApartmentNumber : null,
                BuildingId = u.Apartment != null ? u.Apartment.BuildingId : null,
                BuildingNumber = u.Apartment != null && u.Apartment.Building != null ? u.Apartment.Building.BuildingNumber : null,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt ?? u.CreatedAt
            })
            .ToListAsync();

        return members;
    }

    public async Task<MemberListDto?> GetMemberByIdAsync(int id)
    {
        var member = await _context.Users
            .Where(u => u.Id == id && !u.IsDeleted)
            .Include(u => u.Apartment)
                .ThenInclude(a => a!.Building)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Select(u => new MemberListDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                PhoneNumber = u.Phone,
                Role = u.UserRoles.Any() ? u.UserRoles.First().Role.Name : "User",
                IsActive = u.IsActive,
                ApartmentId = u.ApartmentId,
                ApartmentNumber = u.Apartment != null ? u.Apartment.ApartmentNumber : null,
                BuildingId = u.Apartment != null ? u.Apartment.BuildingId : null,
                BuildingNumber = u.Apartment != null && u.Apartment.Building != null ? u.Apartment.Building.BuildingNumber : null,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt ?? u.CreatedAt
            })
            .FirstOrDefaultAsync();

        return member;
    }

    public async Task<MemberListDto> CreateMemberAsync(CreateMemberDto dto)
    {
        // Validation email unique (seulement si email fourni)
        if (!string.IsNullOrEmpty(dto.Email) && await EmailExistsAsync(dto.Email))
        {
            throw new InvalidOperationException($"Un compte avec l'email {dto.Email} existe déjà.");
        }

        // Vérifier que l'appartement existe
        if (dto.ApartmentId.HasValue)
        {
            var apartmentExists = await _context.Apartments.AnyAsync(a => a.Id == dto.ApartmentId.Value && !a.IsDeleted);
            if (!apartmentExists)
            {
                throw new InvalidOperationException("Appartement introuvable.");
            }
        }

        // Générer mot de passe temporaire
        var temporaryPassword = GenerateTemporaryPassword();
        var passwordHash = HashPassword(temporaryPassword);

        var newUser = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = string.IsNullOrEmpty(dto.Email) ? null : dto.Email,
            Phone = dto.PhoneNumber,
            PasswordHash = passwordHash,
            IsActive = true,
            ApartmentId = dto.ApartmentId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        // Ajouter le rôle
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == dto.Role);
        if (role != null)
        {
            var userRole = new UserRole
            {
                UserId = newUser.Id,
                RoleId = role.Id,
                AssignedAt = DateTime.UtcNow
            };
            _context.Set<UserRole>().Add(userRole);
            await _context.SaveChangesAsync();
        }

        // Envoyer email de bienvenue avec mot de passe temporaire (seulement si email fourni)
        if (!string.IsNullOrEmpty(newUser.Email))
        {
            try
            {
                await _emailService.SendWelcomeEmailAsync(newUser.Email, newUser.FirstName, temporaryPassword);
            }
            catch (Exception ex)
            {
                // Log l'erreur mais ne pas bloquer la création
                Console.WriteLine($"Erreur envoi email bienvenue: {ex.Message}");
            }
        }

        // Retourner le membre créé avec ses relations
        var createdMember = await GetMemberByIdAsync(newUser.Id);
        return createdMember!;
    }

    public async Task<MemberListDto> UpdateMemberAsync(int id, UpdateMemberDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

        if (user == null)
        {
            throw new InvalidOperationException("Adhérent introuvable.");
        }

        // Validation email unique (exclure l'utilisateur actuel, seulement si email fourni)
        if (!string.IsNullOrEmpty(dto.Email) && await EmailExistsAsync(dto.Email, id))
        {
            throw new InvalidOperationException($"Un autre compte avec l'email {dto.Email} existe déjà.");
        }

        // Vérifier que l'appartement existe
        if (dto.ApartmentId.HasValue)
        {
            var apartmentExists = await _context.Apartments.AnyAsync(a => a.Id == dto.ApartmentId.Value && !a.IsDeleted);
            if (!apartmentExists)
            {
                throw new InvalidOperationException("Appartement introuvable.");
            }
        }

        // Mise à jour des champs
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Email = string.IsNullOrEmpty(dto.Email) ? null : dto.Email;
        user.Phone = dto.PhoneNumber;
        user.IsActive = dto.IsActive;
        user.ApartmentId = dto.ApartmentId;
        user.UpdatedAt = DateTime.UtcNow;

        // Mise à jour du rôle si changé
        var currentUserRole = await _context.Set<UserRole>()
            .Include(ur => ur.Role)
            .FirstOrDefaultAsync(ur => ur.UserId == id);
        
        if (currentUserRole != null && currentUserRole.Role.Name != dto.Role)
        {
            // Supprimer l'ancien rôle
            _context.Set<UserRole>().Remove(currentUserRole);
            
            // Ajouter le nouveau rôle
            var newRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == dto.Role);
            if (newRole != null)
            {
                var newUserRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = newRole.Id,
                    AssignedAt = DateTime.UtcNow
                };
                _context.Set<UserRole>().Add(newUserRole);
            }
        }
        else if (currentUserRole == null)
        {
            // Ajouter le rôle s'il n'existe pas
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == dto.Role);
            if (role != null)
            {
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id,
                    AssignedAt = DateTime.UtcNow
                };
                _context.Set<UserRole>().Add(userRole);
            }
        }

        await _context.SaveChangesAsync();

        // Retourner le membre mis à jour avec ses relations
        var updatedMember = await GetMemberByIdAsync(user.Id);
        return updatedMember!;
    }

    public async Task<bool> DeleteMemberAsync(int id)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

        if (user == null)
        {
            return false;
        }

        // Soft delete
        user.IsDeleted = true;
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ContactMemberAsync(int memberId, ContactMemberDto dto)
    {
        var member = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == memberId && !u.IsDeleted);

        if (member == null)
        {
            throw new InvalidOperationException("Adhérent introuvable.");
        }

        // Envoyer l'email via le service d'email
        var success = await _emailService.SendEmailAsync(
            toEmail: member.Email,
            toName: $"{member.FirstName} {member.LastName}",
            subject: dto.Subject,
            body: dto.Body
        );

        return success;
    }

    public string GenerateTemporaryPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%";
        var random = new Random();
        var password = new char[12];

        for (int i = 0; i < password.Length; i++)
        {
            password[i] = chars[random.Next(chars.Length)];
        }

        return new string(password);
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
    {
        var query = _context.Users.Where(u => u.Email == email && !u.IsDeleted);

        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<bool> ApartmentAlreadyAssignedAsync(int apartmentId, int? excludeUserId = null)
    {
        var query = _context.Users.Where(u => u.ApartmentId == apartmentId && !u.IsDeleted);

        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }

        return await query.AnyAsync();
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return BitConverter.ToString(hashedBytes).Replace("-", "");
    }
}
