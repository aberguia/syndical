using System.ComponentModel.DataAnnotations;

namespace GestionSyndicale.Core.DTOs;

/// <summary>
/// DTO pour la liste des membres (adhérents/admins)
/// </summary>
public class MemberListDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int? ApartmentId { get; set; }
    public string? ApartmentNumber { get; set; }
    public int? BuildingId { get; set; }
    public string? BuildingNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO pour créer un nouveau membre
/// </summary>
public class CreateMemberDto
{
    [Required(ErrorMessage = "Le prénom est obligatoire")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom est obligatoire")]
    public string LastName { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Format d'email invalide")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Le téléphone est obligatoire")]
    public string PhoneNumber { get; set; } = string.Empty;

    public int? ApartmentId { get; set; }

    [Required(ErrorMessage = "Le rôle est obligatoire")]
    public string Role { get; set; } = "Adherent"; // Adherent ou Admin
}

/// <summary>
/// DTO pour modifier un membre
/// </summary>
public class UpdateMemberDto
{
    [Required(ErrorMessage = "Le prénom est obligatoire")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom est obligatoire")]
    public string LastName { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Format d'email invalide")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Le téléphone est obligatoire")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le rôle est obligatoire")]
    public string Role { get; set; } = string.Empty;

    public int? ApartmentId { get; set; }

    public bool IsActive { get; set; }
}

/// <summary>
/// DTO pour contacter un membre par email
/// </summary>
public class ContactMemberDto
{
    [Required(ErrorMessage = "Le sujet est obligatoire")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le message est obligatoire")]
    public string Body { get; set; } = string.Empty;
}
