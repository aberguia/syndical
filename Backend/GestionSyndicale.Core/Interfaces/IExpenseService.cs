using GestionSyndicale.Core.DTOs.Expense;

namespace GestionSyndicale.Core.Interfaces;

/// <summary>
/// Service de gestion des dépenses
/// </summary>
public interface IExpenseService
{
    Task<(bool Success, int ExpenseId, string Message)> CreateExpenseAsync(CreateExpenseDto dto, int recordedByUserId);
    Task<bool> AddExpenseAttachmentAsync(int expenseId, Stream fileStream, string fileName, string contentType, int uploadedByUserId);
    Task<ExpenseDetailDto?> GetExpenseByIdAsync(int expenseId);
    Task<List<ExpenseDetailDto>> GetExpensesAsync(DateTime? fromDate = null, DateTime? toDate = null, int? categoryId = null, int page = 1, int pageSize = 10);
    Task<decimal> GetTotalExpensesAsync(DateTime fromDate, DateTime toDate);
}
