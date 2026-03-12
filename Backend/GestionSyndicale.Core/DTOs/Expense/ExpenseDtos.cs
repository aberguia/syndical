namespace GestionSyndicale.Core.DTOs.Expense;

/// <summary>
/// DTO pour créer une dépense
/// </summary>
public class CreateExpenseDto
{
    public int? CategoryId { get; set; } // Deprecated - use SupplierId
    public int? SupplierId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO de détail d'une dépense
/// </summary>
public class ExpenseDetailDto
{
    public int Id { get; set; }
    public int? CategoryId { get; set; } // Deprecated - use Supplier.ServiceCategory
    public string? CategoryName { get; set; }
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public DateTime RecordedAt { get; set; }
    public string RecordedByName { get; set; } = string.Empty;
    public string? InvoiceNumber { get; set; }
    public string? Notes { get; set; }
    public List<AttachmentDto> Attachments { get; set; } = new();
}

/// <summary>
/// DTO pour une pièce jointe
/// </summary>
public class AttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
}

/// <summary>
/// DTO pour modifier une dépense
/// </summary>
public class UpdateExpenseDto
{
    public DateTime ExpenseDate { get; set; }
    public int? CategoryId { get; set; } // Deprecated - use SupplierId
    public int? SupplierId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? InvoiceNumber { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO pour la liste des dépenses avec filtres
/// </summary>
public class ExpenseListDto
{
    public int Id { get; set; }
    public DateTime ExpenseDate { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? InvoiceNumber { get; set; }
    public string? SupplierName { get; set; }
    public int AttachmentsCount { get; set; }
    public DateTime RecordedAt { get; set; }
    public string RecordedByName { get; set; } = string.Empty;
}

/// <summary>
/// DTO pour la réponse paginée avec totaux
/// </summary>
public class ExpensePagedResultDto
{
    public List<ExpenseListDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public decimal TotalAmount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

/// <summary>
/// DTO pour le résumé (reporting)
/// </summary>
public class ExpenseSummaryDto
{
    public int Year { get; set; }
    public decimal TotalYearAmount { get; set; }
    public List<MonthlyExpenseDto> TotalsByMonth { get; set; } = new();
    public List<CategoryExpenseDto> TotalsByCategory { get; set; } = new();
}

public class MonthlyExpenseDto
{
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class CategoryExpenseDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
