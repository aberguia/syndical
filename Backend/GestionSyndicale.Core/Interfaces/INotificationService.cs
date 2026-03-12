namespace GestionSyndicale.Core.Interfaces;

/// <summary>
/// Service de notifications internes
/// </summary>
public interface INotificationService
{
    Task CreateNotificationAsync(int userId, string title, string message, string type, string? relatedEntityType = null, int? relatedEntityId = null);
    Task CreateNotificationForAllMembersAsync(string title, string message, string type);
    Task<List<Entities.Notification>> GetUserNotificationsAsync(int userId, bool unreadOnly = false, int page = 1, int pageSize = 20);
    Task<bool> MarkAsReadAsync(int notificationId, int userId);
    Task<int> GetUnreadCountAsync(int userId);
}
