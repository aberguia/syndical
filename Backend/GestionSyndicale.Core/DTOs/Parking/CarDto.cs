using GestionSyndicale.Core.Entities;

namespace GestionSyndicale.Core.DTOs.Parking;

/// <summary>
/// DTO pour lister les voitures
/// </summary>
public class CarListDto
{
    public int Id { get; set; }
    public CarBrand Brand { get; set; }
    public string BrandDisplay => Brand switch
    {
        CarBrand.Dacia => "Dacia",
        CarBrand.Renault => "Renault",
        CarBrand.Peugeot => "Peugeot",
        CarBrand.Citroen => "Citroën",
        CarBrand.Hyundai => "Hyundai",
        CarBrand.Kia => "Kia",
        CarBrand.Toyota => "Toyota",
        CarBrand.Volkswagen => "Volkswagen",
        CarBrand.Mercedes => "Mercedes",
        CarBrand.BMW => "BMW",
        CarBrand.Audi => "Audi",
        CarBrand.Ford => "Ford",
        CarBrand.Fiat => "Fiat",
        CarBrand.Nissan => "Nissan",
        CarBrand.Suzuki => "Suzuki",
        CarBrand.Opel => "Opel",
        CarBrand.Seat => "Seat",
        CarBrand.Skoda => "Skoda",
        CarBrand.Mazda => "Mazda",
        CarBrand.Mitsubishi => "Mitsubishi",
        CarBrand.Autre => "Autre",
        _ => Brand.ToString()
    };
    public int PlatePart1 { get; set; }
    public string PlatePart2 { get; set; } = string.Empty;
    public int PlatePart3 { get; set; }
    public string PlateFormatted => $"{PlatePart1} {PlatePart2} {PlatePart3}";
    public CarType CarType { get; set; }
    public string CarTypeDisplay => CarType switch
    {
        CarType.Primary => "Principale",
        CarType.Tenant => "Locataire",
        CarType.Visitor => "Visiteur",
        _ => CarType.ToString()
    };
    public int MemberId { get; set; }
    public string MemberFullName { get; set; } = string.Empty;
    public string? MemberPhone { get; set; }
    public int? BuildingId { get; set; }
    public string? BuildingCode { get; set; }
    public int? ApartmentId { get; set; }
    public string? ApartmentNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO pour créer une voiture
/// </summary>
public class CreateCarDto
{
    public CarBrand Brand { get; set; }
    public int PlatePart1 { get; set; }
    public string PlatePart2 { get; set; } = string.Empty;
    public int PlatePart3 { get; set; }
    public CarType CarType { get; set; }
    public int MemberId { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO pour modifier une voiture
/// </summary>
public class UpdateCarDto
{
    public CarBrand Brand { get; set; }
    public int PlatePart1 { get; set; }
    public string PlatePart2 { get; set; } = string.Empty;
    public int PlatePart3 { get; set; }
    public CarType CarType { get; set; }
    public int MemberId { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO pour lookup adhérent (select)
/// </summary>
public class MemberLookupDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? BuildingCode { get; set; }
    public string? ApartmentNumber { get; set; }
    public string DisplayText => $"{FullName} — {BuildingCode} — Appt {ApartmentNumber}";
}
