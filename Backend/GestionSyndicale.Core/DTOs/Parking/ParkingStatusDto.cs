namespace GestionSyndicale.Core.DTOs.Parking;

public class ParkingStatusDto
{
    public int TotalPlaces { get; set; }
    public int CurrentCars { get; set; }
    public int AvailablePlaces { get; set; }
    public string Status { get; set; } = string.Empty; // "OK", "Plein", "Dépassé"
    public DateTime UpdatedAt { get; set; }
}

public class IncrementDecrementDto
{
    public int Count { get; set; } = 1;
}

public class SetCurrentCarsDto
{
    public int CurrentCars { get; set; }
}
