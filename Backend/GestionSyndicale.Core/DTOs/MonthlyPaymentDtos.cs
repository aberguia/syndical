using System.ComponentModel.DataAnnotations;

namespace GestionSyndicale.Core.DTOs;

/// <summary>
/// DTO pour récupérer les mois payés d'un appartement pour une année donnée
/// </summary>
public class ApartmentPaidMonthsDto
{
    public int ApartmentId { get; set; }
    public int Year { get; set; }
    public List<int> PaidMonths { get; set; } = new(); // Mois payés (1-12)
    
    // Informations globales pour validation chronologique
    public int? FirstUnpaidYear { get; set; }
    public int? FirstUnpaidMonth { get; set; }
}

/// <summary>
/// DTO pour créer un paiement mensuel
/// </summary>
public class CreateMonthlyPaymentDto
{
    [Required]
    public int ApartmentId { get; set; }

    [Required]
    [Range(2020, 2100, ErrorMessage = "L'année doit être entre 2020 et 2100")]
    public int Year { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Veuillez sélectionner au moins un mois")]
    public List<int> Months { get; set; } = new(); // Liste des mois à payer (1-12)
}

/// <summary>
/// DTO pour le résumé des paiements d'un appartement (optimisé pour affichage liste)
/// </summary>
public class ApartmentPaymentSummaryDto
{
    public int ApartmentId { get; set; }
    public int PaidMonthsCount { get; set; }         // Mois payés dans l'année courante
    public int PreviousYearsUnpaidCount { get; set; } // Mois impayés des années précédentes
}

/// <summary>
/// DTO pour le résumé des paiements de tous les appartements pour une année
/// </summary>
public class PaymentsSummaryDto
{
    public int Year { get; set; }
    public List<ApartmentPaymentSummaryDto> Apartments { get; set; } = new();
}

/// <summary>
/// DTO pour le résumé des paiements par immeuble
/// </summary>
public class BuildingPaymentSummaryDto
{
    public int BuildingId { get; set; }
    public int ApartmentsCount { get; set; }
    public int TotalPaidMonths { get; set; } // Somme des mois payés de tous les appartements
}

/// <summary>
/// DTO pour le résumé des paiements de tous les immeubles pour une année
/// </summary>
public class BuildingsPaymentsSummaryDto
{
    public int Year { get; set; }
    public List<BuildingPaymentSummaryDto> Buildings { get; set; } = new();
}

