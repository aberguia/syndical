using GestionSyndicale.Core.DTOs;
using GestionSyndicale.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GestionSyndicale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MembersController : ControllerBase
{
    private readonly IMemberService _memberService;
    private readonly IAuditService _auditService;

    public MembersController(IMemberService memberService, IAuditService auditService)
    {
        _memberService = memberService;
        _auditService = auditService;
    }

    /// <summary>
    /// Récupère tous les adhérents (Admin/SuperAdmin)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetAllMembers()
    {
        try
        {
            var members = await _memberService.GetAllMembersAsync();
            return Ok(members);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Erreur lors de la récupération des adhérents: {ex.Message}" });
        }
    }

    /// <summary>
    /// Récupère un adhérent par ID (Admin/SuperAdmin)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetMemberById(int id)
    {
        try
        {
            var member = await _memberService.GetMemberByIdAsync(id);
            if (member == null)
            {
                return NotFound(new { message = "Adhérent introuvable." });
            }
            return Ok(member);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Erreur lors de la récupération de l'adhérent: {ex.Message}" });
        }
    }

    /// <summary>
    /// Crée un nouvel adhérent (SuperAdmin uniquement)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CreateMember([FromBody] CreateMemberDto dto)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var member = await _memberService.CreateMemberAsync(dto);

            await _auditService.LogAsync(
                userId,
                "CreateMember",
                "User",
                member.Id,
                null,
                $"Adhérent créé: {member.FirstName} {member.LastName} ({member.Email})",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
            );

            return CreatedAtAction(nameof(GetMemberById), new { id = member.Id }, member);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Erreur lors de la création de l'adhérent: {ex.Message}" });
        }
    }

    /// <summary>
    /// Met à jour un adhérent (SuperAdmin uniquement)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateMember(int id, [FromBody] UpdateMemberDto dto)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var member = await _memberService.UpdateMemberAsync(id, dto);

            await _auditService.LogAsync(
                userId,
                "UpdateMember",
                "User",
                member.Id,
                null,
                $"Adhérent modifié: {member.FirstName} {member.LastName}",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
            );

            return Ok(member);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Erreur lors de la modification de l'adhérent: {ex.Message}" });
        }
    }

    /// <summary>
    /// Supprime un adhérent (SuperAdmin uniquement)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteMember(int id)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var success = await _memberService.DeleteMemberAsync(id);

            if (!success)
            {
                return NotFound(new { message = "Adhérent introuvable." });
            }

            await _auditService.LogAsync(
                userId,
                "DeleteMember",
                "User",
                id,
                null,
                "Adhérent supprimé (soft delete)",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
            );

            return Ok(new { message = "Adhérent supprimé avec succès." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Erreur lors de la suppression de l'adhérent: {ex.Message}" });
        }
    }

    /// <summary>
    /// Envoie un email à un adhérent (Admin/SuperAdmin)
    /// </summary>
    [HttpPost("{id}/contact")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> ContactMember(int id, [FromBody] ContactMemberDto dto)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var success = await _memberService.ContactMemberAsync(id, dto);

            if (!success)
            {
                return StatusCode(500, new { message = "Échec de l'envoi de l'email." });
            }

            await _auditService.LogAsync(
                userId,
                "ContactMember",
                "User",
                id,
                null,
                $"Email envoyé à l'adhérent - Sujet: {dto.Subject}",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
            );

            return Ok(new { message = "Email envoyé avec succès." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Erreur lors de l'envoi de l'email: {ex.Message}" });
        }
    }
}
