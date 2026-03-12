namespace GestionSyndicale.Core.DTOs.Payment;

/// <summary>
/// DTO pour enregistrer un paiement
/// </summary>
public class CreatePaymentDto
{
    public int ApartmentId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO de détail d'un paiement
/// </summary>
public class PaymentDetailDto
{
    public int Id { get; set; }
    public int ApartmentId { get; set; }
    public string BuildingNumber { get; set; } = string.Empty;
    public string ApartmentNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public DateTime PaymentDate { get; set; }
    public DateTime RecordedAt { get; set; }
    public string RecordedByName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? ReceiptFilePath { get; set; }
    public List<PaymentAllocationDto> Allocations { get; set; } = new();
}

/// <summary>
/// DTO d'allocation d'un paiement
/// </summary>
public class PaymentAllocationDto
{
    public string ChargeName { get; set; } = string.Empty;
    public decimal AllocatedAmount { get; set; }
    public DateTime AllocatedAt { get; set; }
}

/// <summary>
/// DTO pour la situation financière d'un appartement
/// </summary>
public class ApartmentBalanceDto
{
    public int ApartmentId { get; set; }
    public string BuildingNumber { get; set; } = string.Empty;
    public string ApartmentNumber { get; set; } = string.Empty;
    public decimal TotalDue { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal Balance { get; set; }
    public List<CallForFundsDto> PendingCalls { get; set; } = new();
    public List<PaymentDetailDto> RecentPayments { get; set; } = new();
}

/// <summary>
/// DTO pour un appel de fonds
/// </summary>
public class CallForFundsDto
{
    public int Id { get; set; }
    public string ChargeName { get; set; } = string.Empty;
    public decimal AmountDue { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal AmountRemaining { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
