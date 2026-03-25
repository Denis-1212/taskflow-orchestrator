namespace TaskFlow.Services.Notification.Application.Services;

using Domain;

using Infrastructure;

using Microsoft.EntityFrameworkCore;

using Shared.Kernel;

using Notification = Domain.Notification;

public class NotificationService : INotificationService
{

    #region Fields

    private readonly NotificationDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    #endregion

    #region Constructors

    public NotificationService(NotificationDbContext context, ILogger<NotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #endregion

    #region Methods

    public async Task<Result> CreateInAppNotificationAsync(Guid userId, string type, string title, string content, string metadata)
    {
        _logger.LogInformation("Creating notification for user {UserId}: {Title}", userId, title);

        if (!Enum.TryParse(type, true, out NotificationType notificationType))
        {
            return Result.Failure(Error.Validation($"Invalid notification type: {type}"));
        }

        var notification = new Notification(userId, notificationType, title, content, metadata);

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<IEnumerable<NotificationResult>>> GetUserNotificationsAsync(
        Guid userId,
        bool unreadOnly = false,
        int page = 1,
        int pageSize = 20)
    {
        IOrderedQueryable<Notification> query = _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt);

        if (unreadOnly)
        {
            query = (IOrderedQueryable<Notification>)query.Where(n => !n.IsRead).Cast<Notification>();
        }

        List<Notification> notifications = await query
                                               .Skip((page - 1) * pageSize)
                                               .Take(pageSize)
                                               .ToListAsync();

        return notifications.Select(MapToResult).ToList();
    }

    public async Task<Result> MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        Notification? notification = await _context.Notifications
                                         .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null)
        {
            return Result.Failure(Error.NotFound("Notification", notificationId));
        }

        notification.MarkAsRead();
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<int>> GetUnreadCountAsync(Guid userId)
    {
        int count = await _context.Notifications
                        .CountAsync(n => n.UserId == userId && !n.IsRead);

        return count;
    }

    private static NotificationResult MapToResult(Notification notification)
    {
        return new NotificationResult(
            notification.Id,
            notification.Type.ToString(),
            notification.Title,
            notification.Content,
            notification.Metadata,
            notification.IsRead,
            notification.CreatedAt);
    }

    #endregion

}
