namespace GestionSyndicale.Core.Entities;

/// <summary>
/// Allocation d'un paiement à un ou plusieurs appels de fonds
/// Permet de tracer exactement quel paiement a servi pour quelle charge
/// </summary>
public class PaymentAllocation
{
    public int Id { get; set; }
    public int PaymentId { get; set; }
    public int CallForFundsId { get; set; }
    public decimal AllocatedAmount { get; set; }
    public DateTime AllocatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Payment Payment { get; set; } = null!;
    public CallForFunds CallForFunds { get; set; } = null!;
}
