using GestionSyndicale.Core.DTOs;
using GestionSyndicale.Core.DTOs.Payment;
using GestionSyndicale.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GestionSyndicale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IAuditService _auditService;

    public PaymentsController(IPaymentService paymentService, IAuditService auditService)
    {
        _paymentService = paymentService;
        _auditService = auditService;
    }

    /// <summary>
    /// Enregistrer un paiement (Admin/SuperAdmin)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> RecordPayment([FromBody] CreatePaymentDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var (success, paymentId, message) = await _paymentService.RecordPaymentAsync(dto, userId);

        if (!success)
        {
            return BadRequest(new { message });
        }

        await _auditService.LogAsync(
            userId,
            "CreatePayment",
            "Payment",
            paymentId,
            null,
            $"Amount: {dto.Amount}, Apartment: {dto.ApartmentId}",
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
        );

        return CreatedAtAction(nameof(GetPaymentById), new { id = paymentId }, new { message, paymentId });
    }

    /// <summary>
    /// Obtenir un paiement par ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPaymentById(int id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);

        if (payment == null)
        {
            return NotFound(new { message = "Paiement introuvable." });
        }

        // Vérifier les droits d'accès
        var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var userApartmentId = User.FindFirst("ApartmentId")?.Value;

        if (!userRoles.Contains("SuperAdmin") && !userRoles.Contains("Admin"))
        {
            // Adhérent : ne peut voir que ses propres paiements
            if (userApartmentId == null || payment.ApartmentId != int.Parse(userApartmentId))
            {
                return Forbid();
            }
        }

        return Ok(payment);
    }

    /// <summary>
    /// Obtenir les paiements d'un appartement
    /// </summary>
    [HttpGet("apartment/{apartmentId}")]
    public async Task<IActionResult> GetPaymentsByApartment(int apartmentId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        // Vérifier les droits d'accès
        var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var userApartmentId = User.FindFirst("ApartmentId")?.Value;

        if (!userRoles.Contains("SuperAdmin") && !userRoles.Contains("Admin"))
        {
            // Adhérent : ne peut voir que ses propres paiements
            if (userApartmentId == null || apartmentId != int.Parse(userApartmentId))
            {
                return Forbid();
            }
        }

        var payments = await _paymentService.GetPaymentsByApartmentAsync(apartmentId, page, pageSize);
        return Ok(payments);
    }

    /// <summary>
    /// Obtenir la situation financière d'un appartement
    /// </summary>
    [HttpGet("apartment/{apartmentId}/balance")]
    public async Task<IActionResult> GetApartmentBalance(int apartmentId)
    {
        // Vérifier les droits d'accès
        var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var userApartmentId = User.FindFirst("ApartmentId")?.Value;

        if (!userRoles.Contains("SuperAdmin") && !userRoles.Contains("Admin"))
        {
            // Adhérent : ne peut voir que sa propre situation
            if (userApartmentId == null || apartmentId != int.Parse(userApartmentId))
            {
                return Forbid();
            }
        }

        try
        {
            var balance = await _paymentService.GetApartmentBalanceAsync(apartmentId);
            return Ok(balance);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Télécharger le reçu PDF d'un paiement
    /// </summary>
    [HttpGet("{id}/receipt")]
    public async Task<IActionResult> GetPaymentReceipt(int id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);

        if (payment == null)
        {
            return NotFound(new { message = "Paiement introuvable." });
        }

        // Vérifier les droits d'accès
        var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var userApartmentId = User.FindFirst("ApartmentId")?.Value;

        if (!userRoles.Contains("SuperAdmin") && !userRoles.Contains("Admin"))
        {
            if (userApartmentId == null || payment.ApartmentId != int.Parse(userApartmentId))
            {
                return Forbid();
            }
        }

        var pdfBytes = await _paymentService.GeneratePaymentReceiptPdfAsync(id);
        return File(pdfBytes, "application/pdf", $"Recu_{id}.pdf");
    }

    /// <summary>
    /// Récupère l'état global des paiements pour un appartement
    /// </summary>
    [HttpGet("apartment/{apartmentId}/status")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetPaymentStatus(int apartmentId)
    {
        var status = await _paymentService.GetPaymentStatusAsync(apartmentId);
        return Ok(status);
    }

    /// <summary>
    /// Récupère les mois payés pour un appartement et une année donnée
    /// </summary>
    [HttpGet("apartment/{apartmentId}/year/{year}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetPaidMonths(int apartmentId, int year)
    {
        var paidMonths = await _paymentService.GetPaidMonthsAsync(apartmentId, year);
        var status = await _paymentService.GetPaymentStatusAsync(apartmentId);
        
        var dto = new ApartmentPaidMonthsDto
        {
            ApartmentId = apartmentId,
            Year = year,
            PaidMonths = paidMonths,
            FirstUnpaidYear = status.FirstUnpaidYear,
            FirstUnpaidMonth = status.FirstUnpaidMonth
        };

        return Ok(dto);
    }

    /// <summary>
    /// Obtenir le résumé des paiements de tous les appartements pour une année donnée
    /// </summary>
    [HttpGet("summary/year/{year}")]
    public async Task<IActionResult> GetPaymentsSummaryByYear(int year)
    {
        var summary = await _paymentService.GetPaymentsSummaryByYearAsync(year);
        return Ok(summary);
    }

    /// <summary>
    /// Obtenir le résumé des paiements de tous les immeubles pour une année donnée
    /// </summary>
    [HttpGet("summary/buildings/year/{year}")]
    public async Task<IActionResult> GetBuildingsPaymentsSummaryByYear(int year)
    {
        var summary = await _paymentService.GetBuildingsPaymentsSummaryByYearAsync(year);
        return Ok(summary);
    }

    /// <summary>
    /// Enregistre un paiement mensuel pour plusieurs mois
    /// </summary>
    [HttpPost("apartment")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> CreateMonthlyPayment([FromBody] CreateMonthlyPaymentDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            // Validation : vérifier que les mois sont payés dans l'ordre chronologique
            var validationError = await _paymentService.ValidateChronologicalPaymentAsync(dto);
            if (!string.IsNullOrEmpty(validationError))
            {
                return BadRequest(new { message = validationError });
            }

            await _paymentService.CreateMonthlyPaymentsAsync(dto, userId);

            await _auditService.LogAsync(
                userId,
                "CreateMonthlyPayment",
                "MonthlyPayment",
                dto.ApartmentId,
                null,
                $"Year: {dto.Year}, Months: {string.Join(", ", dto.Months)}",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
            );

            return Ok(new { message = "Paiements enregistrés avec succès" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de l'enregistrement des paiements", error = ex.Message });
        }
    }

    /// <summary>
    /// Annuler (décocher) des paiements mensuels (SuperAdmin/Admin uniquement)
    /// </summary>
    [HttpPost("monthly/cancel")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> CancelMonthlyPayments([FromBody] CancelMonthlyPaymentDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            await _paymentService.CancelMonthlyPaymentsAsync(dto.ApartmentId, dto.Year, dto.Months, userId);

            await _auditService.LogAsync(
                userId,
                "CancelMonthlyPayment",
                "MonthlyPayment",
                dto.ApartmentId,
                null,
                $"Year: {dto.Year}, Months: {string.Join(", ", dto.Months)}",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
            );

            return Ok(new { message = "Paiements annulés avec succès" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de l'annulation des paiements", error = ex.Message });
        }
    }
}

public class CancelMonthlyPaymentDto
{
    public int ApartmentId { get; set; }
    public int Year { get; set; }
    public List<int> Months { get; set; } = new();
}
