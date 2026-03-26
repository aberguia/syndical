using GestionSyndicale.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GestionSyndicale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class ReceiptsController : ControllerBase
{
    private readonly IPdfService _pdfService;
    private readonly ILogger<ReceiptsController> _logger;

    public ReceiptsController(IPdfService pdfService, ILogger<ReceiptsController> logger)
    {
        _pdfService = pdfService;
        _logger = logger;
    }

    /// <summary>
    /// Générer un reçu PDF pour un appartement avec les mois payés sur les années sélectionnées
    /// </summary>
    [HttpPost("apartment/{apartmentId}")]
    public async Task<IActionResult> GenerateApartmentReceipt(int apartmentId, [FromBody] GenerateReceiptRequest request)
    {
        try
        {
            if (request.Years == null || !request.Years.Any())
            {
                return BadRequest(new { message = "Au moins une année doit être sélectionnée" });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var pdfBytes = await _pdfService.GenerateApartmentReceiptAsync(apartmentId, request.Years, userId, request.Lang);
            
            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                return BadRequest(new { message = "Impossible de générer le reçu. Vérifiez que l'appartement existe et qu'un adhérent est associé." });
            }

            var yearsSuffix = string.Join("_", request.Years.OrderBy(y => y));
            var filename = $"recu_appartement_{apartmentId}_{yearsSuffix}.pdf";

            return File(pdfBytes, "application/pdf", filename);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating receipt for apartment {ApartmentId}", apartmentId);
            return StatusCode(500, new { message = "Erreur lors de la génération du reçu" });
        }
    }
}

public class GenerateReceiptRequest
{
    public List<int> Years { get; set; } = new();
    public string Lang { get; set; } = "fr";
}
