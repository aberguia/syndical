using GestionSyndicale.Core.DTOs;

namespace GestionSyndicale.Core.Interfaces;

public interface IMemberService
{
    /// <summary>
    /// Récupère tous les adhérents avec leurs informations de bâtiment et appartement
    /// </summary>
    Task<IEnumerable<MemberListDto>> GetAllMembersAsync();

    /// <summary>
    /// Récupère un adhérent par son ID
    /// </summary>
    Task<MemberListDto?> GetMemberByIdAsync(int id);

    /// <summary>
    /// Crée un nouvel adhérent avec un mot de passe temporaire généré automatiquement
    /// Envoie un email de bienvenue avec les identifiants
    /// </summary>
    Task<MemberListDto> CreateMemberAsync(CreateMemberDto dto);

    /// <summary>
    /// Met à jour un adhérent existant
    /// </summary>
    Task<MemberListDto> UpdateMemberAsync(int id, UpdateMemberDto dto);

    /// <summary>
    /// Supprime un adhérent (soft delete - désactivation)
    /// </summary>
    Task<bool> DeleteMemberAsync(int id);

    /// <summary>
    /// Envoie un email à un adhérent spécifique
    /// </summary>
    Task<bool> ContactMemberAsync(int memberId, ContactMemberDto dto);

    /// <summary>
    /// Génère un mot de passe temporaire aléatoire
    /// </summary>
    string GenerateTemporaryPassword();

    /// <summary>
    /// Vérifie si un email existe déjà (pour validation unicité)
    /// </summary>
    Task<bool> EmailExistsAsync(string email, int? excludeUserId = null);

    /// <summary>
    /// Vérifie si un appartement est déjà attribué à un autre adhérent
    /// </summary>
    Task<bool> ApartmentAlreadyAssignedAsync(int apartmentId, int? excludeUserId = null);
}
