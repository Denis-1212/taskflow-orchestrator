namespace TaskFlow.Services.Notification.Application.Services;

using Domain;

using Shared.Kernel;

public interface INotificationService
{

    #region Methods

    Task<Result> CreateInAppNotificationAsync(Guid userId, NotificationType type, string title, string content, string metadata);
    Task<Result<IEnumerable<NotificationResult>>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false, int page = 1, int pageSize = 20);
    Task<Result> MarkAsReadAsync(Guid notificationId, Guid userId);
    Task<Result<int>> GetUnreadCountAsync(Guid userId);
    Task<Result> MarkAsReadAllAsync();

    #endregion

}

public record NotificationResult(
    Guid Id,
    string Type,
    string Title,
    string Content,
    string Metadata,
    bool IsRead,
    DateTime CreatedAt);
