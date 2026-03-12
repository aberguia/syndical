using GestionSyndicale.Core.DTOs.Supplier;
using GestionSyndicale.Core.Entities;
using GestionSyndicale.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionSyndicale.Infrastructure.Services;

public interface ISupplierService
{
    Task<List<SupplierListDto>> GetAllAsync(string? category = null, string? search = null);
    Task<SupplierDetailDto?> GetByIdAsync(int id);
    Task<SupplierDetailDto> CreateAsync(CreateSupplierDto dto, int userId);
    Task<SupplierDetailDto> UpdateAsync(int id, UpdateSupplierDto dto, int userId);
    Task DeleteAsync(int id);
    Task<List<SupplierLookupDto>> GetLookupsAsync(string? category = null);
    Task<bool> IsNameCategoryUniqueAsync(string name, string category, int? excludeId = null);
}

public class SupplierService : ISupplierService
{
    private readonly ApplicationDbContext _context;

    public SupplierService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SupplierListDto>> GetAllAsync(string? category = null, string? search = null)
    {
        var query = _context.Suppliers
            .Where(s => !s.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(s => s.ServiceCategory == category);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s => s.Name.Contains(search) || 
                                     (s.Description != null && s.Description.Contains(search)));
        }

        var suppliers = await query
            .Select(s => new SupplierListDto
            {
                Id = s.Id,
                Name = s.Name,
                ServiceCategory = s.ServiceCategory,
                Description = s.Description,
                Phone = s.Phone,
                Email = s.Email,
                IsActive = s.IsActive,
                CreatedOn = s.CreatedOn,
                ExpenseCount = s.Expenses.Count(e => !e.IsDeleted)
            })
            .OrderBy(s => s.ServiceCategory)
            .ThenBy(s => s.Name)
            .ToListAsync();

        return suppliers;
    }

    public async Task<SupplierDetailDto?> GetByIdAsync(int id)
    {
        var supplier = await _context.Suppliers
            .Where(s => s.Id == id && !s.IsDeleted)
            .Include(s => s.CreatedByUser)
            .Include(s => s.UpdatedByUser)
            .Select(s => new SupplierDetailDto
            {
                Id = s.Id,
                Name = s.Name,
                ServiceCategory = s.ServiceCategory,
                Description = s.Description,
                Phone = s.Phone,
                Email = s.Email,
                Address = s.Address,
                IsActive = s.IsActive,
                CreatedOn = s.CreatedOn,
                CreatedByName = s.CreatedByUser != null ? 
                    s.CreatedByUser.FirstName + " " + s.CreatedByUser.LastName : null,
                UpdatedOn = s.UpdatedOn,
                UpdatedByName = s.UpdatedByUser != null ?
                    s.UpdatedByUser.FirstName + " " + s.UpdatedByUser.LastName : null
            })
            .FirstOrDefaultAsync();

        return supplier;
    }

    public async Task<SupplierDetailDto> CreateAsync(CreateSupplierDto dto, int userId)
    {
        // Check uniqueness
        if (!await IsNameCategoryUniqueAsync(dto.Name, dto.ServiceCategory))
        {
            throw new InvalidOperationException($"Un fournisseur avec le nom '{dto.Name}' existe déjà dans la catégorie '{dto.ServiceCategory}'.");
        }

        var supplier = new Supplier
        {
            Name = dto.Name,
            ServiceCategory = dto.ServiceCategory,
            Description = dto.Description,
            Phone = dto.Phone,
            Email = dto.Email,
            Address = dto.Address,
            IsActive = true,
            CreatedOn = DateTime.UtcNow,
            CreatedByUserId = userId
        };

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(supplier.Id))!;
    }

    public async Task<SupplierDetailDto> UpdateAsync(int id, UpdateSupplierDto dto, int userId)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        if (supplier == null)
        {
            throw new KeyNotFoundException("Fournisseur introuvable.");
        }

        // Check uniqueness (excluding current supplier)
        if (!await IsNameCategoryUniqueAsync(dto.Name, dto.ServiceCategory, id))
        {
            throw new InvalidOperationException($"Un fournisseur avec le nom '{dto.Name}' existe déjà dans la catégorie '{dto.ServiceCategory}'.");
        }

        supplier.Name = dto.Name;
        supplier.ServiceCategory = dto.ServiceCategory;
        supplier.Description = dto.Description;
        supplier.Phone = dto.Phone;
        supplier.Email = dto.Email;
        supplier.Address = dto.Address;
        supplier.IsActive = dto.IsActive;
        supplier.UpdatedOn = DateTime.UtcNow;
        supplier.UpdatedByUserId = userId;

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(id))!;
    }

    public async Task DeleteAsync(int id)
    {
        var supplier = await _context.Suppliers
            .Include(s => s.Expenses.Where(e => !e.IsDeleted))
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        if (supplier == null)
        {
            throw new KeyNotFoundException("Fournisseur introuvable.");
        }

        // Check if supplier is used in expenses
        if (supplier.Expenses.Any())
        {
            throw new InvalidOperationException($"Ce fournisseur est utilisé dans {supplier.Expenses.Count} dépense(s) et ne peut pas être supprimé.");
        }

        supplier.IsDeleted = true;
        supplier.UpdatedOn = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<List<SupplierLookupDto>> GetLookupsAsync(string? category = null)
    {
        var query = _context.Suppliers
            .Where(s => !s.IsDeleted && s.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(s => s.ServiceCategory == category);
        }

        var lookups = await query
            .Select(s => new SupplierLookupDto
            {
                Id = s.Id,
                Name = s.Name,
                ServiceCategory = s.ServiceCategory
            })
            .OrderBy(s => s.Name)
            .ToListAsync();

        return lookups;
    }

    public async Task<bool> IsNameCategoryUniqueAsync(string name, string category, int? excludeId = null)
    {
        var query = _context.Suppliers
            .Where(s => !s.IsDeleted && 
                        s.Name.ToLower() == name.ToLower() && 
                        s.ServiceCategory == category);

        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }
}
