namespace GestionSyndicale.Core.DTOs.User;

/// <summary>
/// DTO pour créer un utilisateur (Admin/SuperAdmin)
/// </summary>
public class CreateUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int? ApartmentId { get; set; }
    public List<string> Roles { get; set; } = new();
}

/// <summary>
/// DTO de détail utilisateur
/// </summary>
public class UserDetailDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsEmailConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<string> Roles { get; set; } = new();
    public ApartmentInfoDto? Apartment { get; set; }
}

/// <summary>
/// DTO d'information d'appartement
/// </summary>
public class ApartmentInfoDto
{
    public int Id { get; set; }
    public string BuildingNumber { get; set; } = string.Empty;
    public string ApartmentNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public decimal Surface { get; set; }
    public int SharesCount { get; set; }
}
