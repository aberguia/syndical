namespace GestionSyndicale.Core.Interfaces;

/// <summary>
/// Service d'audit pour traçabilité
/// </summary>
public interface IAuditService
{
    Task LogAsync(int? userId, string action, string entityType, int? entityId, string? oldValues, string? newValues, string ipAddress);
    Task<List<Entities.AuditLog>> GetAuditLogsAsync(string? entityType = null, int? entityId = null, int? userId = null, DateTime? fromDate = null, int page = 1, int pageSize = 50);
}
