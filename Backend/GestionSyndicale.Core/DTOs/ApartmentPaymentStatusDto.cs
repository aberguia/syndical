namespace GestionSyndicale.Core.DTOs;

/// <summary>
/// DTO pour l'état global des paiements d'un appartement
/// </summary>
public class ApartmentPaymentStatusDto
{
    public int ApartmentId { get; set; }
    
    /// <summary>
    /// Année du premier mois impayé (null si tout est payé)
    /// </summary>
    public int? FirstUnpaidYear { get; set; }
    
    /// <summary>
    /// Mois du premier impayé (1-12, null si tout est payé)
    /// </summary>
    public int? FirstUnpaidMonth { get; set; }
    
    /// <summary>
    /// Année du dernier mois payé (null si aucun paiement)
    /// </summary>
    public int? LastPaidYear { get; set; }
    
    /// <summary>
    /// Mois du dernier paiement (1-12, null si aucun paiement)
    /// </summary>
    public int? LastPaidMonth { get; set; }
}
