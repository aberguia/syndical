namespace GestionSyndicale.Core.DTOs.Settings;

public class ApartmentResidentDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
}

/// <summary>
/// DTO pour retourner un appartement
/// </summary>
public class ApartmentDto
{
    public int Id { get; set; }
    public int BuildingId { get; set; }
    public string BuildingNumber { get; set; } = string.Empty;
    public string? BuildingName { get; set; }
    public string ApartmentNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public decimal Surface { get; set; }
    public int SharesCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<ApartmentResidentDto> Residents { get; set; } = new();
}

/// <summary>
/// DTO pour créer un appartement
/// </summary>
public class CreateApartmentDto
{
    public int BuildingId { get; set; }
    public string ApartmentNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public decimal Surface { get; set; }
    public int SharesCount { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO pour modifier un appartement
/// </summary>
public class UpdateApartmentDto
{
    public int BuildingId { get; set; }
    public string ApartmentNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public decimal Surface { get; set; }
    public int SharesCount { get; set; }
    public bool IsActive { get; set; }
}
