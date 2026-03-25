namespace TaskFlow.Services.Notification.Tests;

using Application.Services;

using Domain;

using FluentAssertions;

using Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

using Shared.Kernel;

using Notification = Domain.Notification;

public class NotificationServiceTests : IDisposable
{

    #region Fields

    private readonly NotificationDbContext _context;
    private readonly Mock<ILogger<NotificationService>> _loggerMock;
    private readonly NotificationService _notificationService;

    #endregion

    #region Constructors

    public NotificationServiceTests()
    {
        _context = TestDatabase.Create();
        _loggerMock = new Mock<ILogger<NotificationService>>();
        _notificationService = new NotificationService(_context, _loggerMock.Object);
    }

    #endregion

    #region Methods

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task CreateInAppNotificationAsync_WithValidData_ShouldCreateNotification()
    {
        // Arrange
        var userId = Guid.NewGuid();
        string type = "TaskAssigned";
        string title = "Test Notification";
        string content = "Test Content";
        string metadata = "{\"taskId\":\"123\"}";

        // Act
        Result result = await _notificationService.CreateInAppNotificationAsync(userId, type, title, content, metadata);

        // Assert
        result.IsSuccess.Should().BeTrue();

        Notification? notification = await _context.Notifications.FirstOrDefaultAsync(n => n.UserId == userId);
        notification.Should().NotBeNull();
        notification!.Title.Should().Be(title);
        notification.Content.Should().Be(content);
        notification.Type.Should().Be(NotificationType.TaskAssigned);
        notification.IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task CreateInAppNotificationAsync_WithInvalidType_ShouldReturnValidationError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        string type = "InvalidType";
        string title = "Test Notification";
        string content = "Test Content";
        string metadata = "{}";

        // Act
        Result result = await _notificationService.CreateInAppNotificationAsync(userId, type, title, content, metadata);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Validation);
        result.Error.Message.Should().Contain("Invalid notification type");
    }

    [Fact]
    public async Task GetUserNotificationsAsync_ShouldReturnUserNotifications()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notification1 = new Notification(userId, NotificationType.TaskAssigned, "Title1", "Content1", "{}");
        var notification2 = new Notification(userId, NotificationType.TaskStatusChanged, "Title2", "Content2", "{}");

        _context.Notifications.AddRange(notification1, notification2);
        await _context.SaveChangesAsync();

        // Act
        Result<IEnumerable<NotificationResult>> result = await _notificationService.GetUserNotificationsAsync(userId, false, 1, 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUserNotificationsAsync_WithUnreadOnly_ShouldReturnOnlyUnread()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var unreadNotification = new Notification(userId, NotificationType.TaskAssigned, "Unread", "Content", "{}");
        var readNotification = new Notification(userId, NotificationType.TaskStatusChanged, "Read", "Content", "{}");
        readNotification.MarkAsRead();

        _context.Notifications.AddRange(unreadNotification, readNotification);
        await _context.SaveChangesAsync();

        // Act
        Result<IEnumerable<NotificationResult>> result = await _notificationService.GetUserNotificationsAsync(userId, true, 1, 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Title.Should().Be("Unread");
    }

    [Fact]
    public async Task GetUserNotificationsAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var userId = Guid.NewGuid();

        for (int i = 1; i <= 15; i++)
        {
            var notification = new Notification(userId, NotificationType.TaskAssigned, $"Title{i}", "Content", "{}");
            _context.Notifications.Add(notification);
        }

        await _context.SaveChangesAsync();

        // Act - page 2 with 5 items per page
        Result<IEnumerable<NotificationResult>> result = await _notificationService.GetUserNotificationsAsync(userId, false, 2, 5);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(5);
    }

    [Fact]
    public async Task MarkAsReadAsync_WithValidNotification_ShouldMarkAsRead()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notification = new Notification(userId, NotificationType.TaskAssigned, "Title", "Content", "{}");
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // Act
        Result result = await _notificationService.MarkAsReadAsync(notification.Id, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();

        Notification? updatedNotification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == notification.Id);
        updatedNotification!.IsRead.Should().BeTrue();
        updatedNotification.ReadAt.Should().NotBeNull();
    }

    [Fact]
    public async Task MarkAsReadAsync_WithWrongUser_ShouldReturnNotFound()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var wrongUserId = Guid.NewGuid();
        var notification = new Notification(ownerId, NotificationType.TaskAssigned, "Title", "Content", "{}");
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // Act
        Result result = await _notificationService.MarkAsReadAsync(notification.Id, wrongUserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task MarkAsReadAsync_WithNonExistentNotification_ShouldReturnNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var nonExistentId = Guid.NewGuid();

        // Act
        Result result = await _notificationService.MarkAsReadAsync(nonExistentId, userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetUnreadCountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var unread1 = new Notification(userId, NotificationType.TaskAssigned, "Title1", "Content", "{}");
        var unread2 = new Notification(userId, NotificationType.TaskStatusChanged, "Title2", "Content", "{}");
        var read = new Notification(userId, NotificationType.TaskAssigned, "Title3", "Content", "{}");
        read.MarkAsRead();

        _context.Notifications.AddRange(unread1, unread2, read);
        await _context.SaveChangesAsync();

        // Act
        Result<int> result = await _notificationService.GetUnreadCountAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    [Fact]
    public async Task GetUnreadCountAsync_WithNoNotifications_ShouldReturnZero()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        Result<int> result = await _notificationService.GetUnreadCountAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    #endregion

}
