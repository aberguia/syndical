using GestionSyndicale.Core.DTOs.Expense;
using GestionSyndicale.Core.Entities;
using GestionSyndicale.Core.Interfaces;
using GestionSyndicale.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionSyndicale.Infrastructure.Services;

/// <summary>
/// Service de gestion des dépenses
/// </summary>
public class ExpenseService : IExpenseService
{
    private readonly ApplicationDbContext _context;

    public ExpenseService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, int ExpenseId, string Message)> CreateExpenseAsync(CreateExpenseDto dto, int recordedByUserId)
    {
        try
        {
            var expense = new Expense
            {
                SupplierId = dto.SupplierId,
                Description = dto.Description,
                Amount = dto.Amount,
                ExpenseDate = dto.ExpenseDate,
                RecordedAt = DateTime.UtcNow,
                RecordedByUserId = recordedByUserId,
                InvoiceNumber = dto.InvoiceNumber,
                Notes = dto.Notes
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            return (true, expense.Id, "Dépense enregistrée avec succès.");
        }
        catch (Exception ex)
        {
            return (false, 0, $"Erreur lors de l'enregistrement de la dépense: {ex.Message}");
        }
    }

    public async Task<bool> AddExpenseAttachmentAsync(int expenseId, Stream fileStream, string fileName, string contentType, int uploadedByUserId)
    {
        var expense = await _context.Expenses.FindAsync(expenseId);
        if (expense == null)
        {
            return false;
        }

        // Créer le répertoire si nécessaire
        var uploadPath = Path.Combine("Uploads", "Expenses", expenseId.ToString());
        Directory.CreateDirectory(uploadPath);

        // Sauvegarder le fichier
        var filePath = Path.Combine(uploadPath, fileName);
        using (var fileStreamOutput = File.Create(filePath))
        {
            await fileStream.CopyToAsync(fileStreamOutput);
        }

        // Créer l'enregistrement
        var attachment = new ExpenseAttachment
        {
            ExpenseId = expenseId,
            FileName = fileName,
            FilePath = filePath,
            FileType = contentType,
            FileSize = new FileInfo(filePath).Length,
            UploadedAt = DateTime.UtcNow,
            UploadedByUserId = uploadedByUserId
        };

        _context.ExpenseAttachments.Add(attachment);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<ExpenseDetailDto?> GetExpenseByIdAsync(int expenseId)
    {
        var expense = await _context.Expenses
            .Include(e => e.Supplier)
            .Include(e => e.RecordedBy)
            .Include(e => e.Attachments)
            .FirstOrDefaultAsync(e => e.Id == expenseId);

        if (expense == null) return null;

        return new ExpenseDetailDto
        {
            Id = expense.Id,
            CategoryName = expense.Supplier?.ServiceCategory,
            SupplierName = expense.Supplier?.Name,
            Description = expense.Description,
            Amount = expense.Amount,
            ExpenseDate = expense.ExpenseDate,
            RecordedAt = expense.RecordedAt,
            RecordedByName = $"{expense.RecordedBy.FirstName} {expense.RecordedBy.LastName}",
            InvoiceNumber = expense.InvoiceNumber,
            Notes = expense.Notes,
            Attachments = expense.Attachments.Select(a => new AttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                FileType = a.FileType,
                FileSize = a.FileSize,
                UploadedAt = a.UploadedAt
            }).ToList()
        };
    }

    public async Task<List<ExpenseDetailDto>> GetExpensesAsync(DateTime? fromDate = null, DateTime? toDate = null, int? categoryId = null, int page = 1, int pageSize = 10)
    {
        var query = _context.Expenses
            .Include(e => e.Supplier)
            .Include(e => e.RecordedBy)
            .Include(e => e.Attachments)
            .AsQueryable();

        if (fromDate.HasValue)
        {
            query = query.Where(e => e.ExpenseDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(e => e.ExpenseDate <= toDate.Value);
        }

        // categoryId filter is deprecated - kept for backward compatibility but doesn't do anything
        // Categories are now accessed through Supplier.ServiceCategory

        var expenses = await query
            .OrderByDescending(e => e.ExpenseDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return expenses.Select(e => new ExpenseDetailDto
        {
            Id = e.Id,
            CategoryName = e.Supplier?.ServiceCategory,
            SupplierName = e.Supplier?.Name,
            Description = e.Description,
            Amount = e.Amount,
            ExpenseDate = e.ExpenseDate,
            RecordedAt = e.RecordedAt,
            RecordedByName = $"{e.RecordedBy.FirstName} {e.RecordedBy.LastName}",
            InvoiceNumber = e.InvoiceNumber,
            Notes = e.Notes,
            Attachments = e.Attachments.Select(a => new AttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                FileType = a.FileType,
                FileSize = a.FileSize,
                UploadedAt = a.UploadedAt
            }).ToList()
        }).ToList();
    }

    public async Task<decimal> GetTotalExpensesAsync(DateTime fromDate, DateTime toDate)
    {
        return await _context.Expenses
            .Where(e => e.ExpenseDate >= fromDate && e.ExpenseDate <= toDate)
            .SumAsync(e => e.Amount);
    }
}
