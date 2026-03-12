using GestionSyndicale.Core.Entities;

namespace GestionSyndicale.Core.Interfaces;

/// <summary>
/// Service de génération de PDF
/// </summary>
public interface IPdfService
{
    Task<byte[]> GeneratePaymentReceiptAsync(int paymentId);
    Task<byte[]> GenerateMonthlyReportAsync(int year, int month);
    Task<byte[]> GenerateAnnualReportAsync(int year);
    Task<byte[]> GenerateApartmentBalanceStatementAsync(int apartmentId);
    Task<byte[]> GenerateApartmentReceiptAsync(int apartmentId, List<int> years, int userId);
    Task<byte[]> GenerateBuildingPaymentsGridAsync(Building building, List<int> years);
}
