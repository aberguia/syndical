namespace GestionSyndicale.Core.DTOs.Settings;

/// <summary>
/// DTO pour retourner un immeuble
/// </summary>
public class BuildingDto
{
    public int Id { get; set; }
    public int ResidenceId { get; set; }
    public string BuildingNumber { get; set; } = string.Empty;
    public string? Name { get; set; }
    public int FloorCount { get; set; }
    public bool IsActive { get; set; }
    public int ApartmentsCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO pour créer un immeuble
/// </summary>
public class CreateBuildingDto
{
    public string BuildingNumber { get; set; } = string.Empty;
    public string? Name { get; set; }
    public int FloorCount { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO pour modifier un immeuble
/// </summary>
public class UpdateBuildingDto
{
    public string BuildingNumber { get; set; } = string.Empty;
    public string? Name { get; set; }
    public int FloorCount { get; set; }
    public bool IsActive { get; set; }
}
