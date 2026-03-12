using GestionSyndicale.Core.DTOs;
using GestionSyndicale.Core.DTOs.Payment;

namespace GestionSyndicale.Core.Interfaces;

/// <summary>
/// Service de gestion des paiements
/// </summary>
public interface IPaymentService
{
    Task<(bool Success, int PaymentId, string Message)> RecordPaymentAsync(CreatePaymentDto dto, int recordedByUserId);
    Task<PaymentDetailDto?> GetPaymentByIdAsync(int paymentId);
    Task<List<PaymentDetailDto>> GetPaymentsByApartmentAsync(int apartmentId, int page = 1, int pageSize = 10);
    Task<ApartmentBalanceDto> GetApartmentBalanceAsync(int apartmentId);
    Task<byte[]> GeneratePaymentReceiptPdfAsync(int paymentId);
    
    // Paiements mensuels
    Task<ApartmentPaymentStatusDto> GetPaymentStatusAsync(int apartmentId);
    Task<List<int>> GetPaidMonthsAsync(int apartmentId, int year);
    Task<string?> ValidateChronologicalPaymentAsync(CreateMonthlyPaymentDto dto);
    Task CreateMonthlyPaymentsAsync(CreateMonthlyPaymentDto dto, int recordedByUserId);
    Task CancelMonthlyPaymentsAsync(int apartmentId, int year, List<int> months, int canceledByUserId);
    Task<PaymentsSummaryDto> GetPaymentsSummaryByYearAsync(int year);
    Task<BuildingsPaymentsSummaryDto> GetBuildingsPaymentsSummaryByYearAsync(int year);
}
