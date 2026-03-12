using GestionSyndicale.Core.DTOs.Supplier;
using GestionSyndicale.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GestionSyndicale.API.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category = null, [FromQuery] string? q = null)
    {
        try
        {
            var suppliers = await _supplierService.GetAllAsync(category, q);
            return Ok(suppliers);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Erreur lors de la récupération des fournisseurs: {ex.Message}" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var supplier = await _supplierService.GetByIdAsync(id);
            if (supplier == null)
            {
                return NotFound(new { message = "Fournisseur introuvable." });
            }
            return Ok(supplier);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Erreur lors de la récupération du fournisseur: {ex.Message}" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupplierDto dto)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var supplier = await _supplierService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = supplier.Id }, supplier);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Erreur lors de la création du fournisseur: {ex.Message}" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierDto dto)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var supplier = await _supplierService.UpdateAsync(id, dto, userId);
            return Ok(supplier);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Erreur lors de la mise à jour du fournisseur: {ex.Message}" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _supplierService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Erreur lors de la suppression du fournisseur: {ex.Message}" });
        }
    }
}

[ApiController]
[Route("api/lookups")]
[Authorize]
public class SupplierLookupsController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SupplierLookupsController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    [HttpGet("suppliers")]
    public async Task<IActionResult> GetSuppliers([FromQuery] string? category = null)
    {
        try
        {
            var suppliers = await _supplierService.GetLookupsAsync(category);
            return Ok(suppliers);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Erreur lors de la récupération des fournisseurs: {ex.Message}" });
        }
    }
}
