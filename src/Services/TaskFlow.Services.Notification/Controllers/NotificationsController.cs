namespace TaskFlow.Services.Notification.Controllers;

using System.Security.Claims;

using Application.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Shared.DTOs;
using Shared.Kernel;

[ApiController]
[Authorize]
[Route("api/notifications")]
public class NotificationsController(INotificationService notificationService) : ControllerBase
{

    #region Methods

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        Guid userId = GetCurrentUserId();

        Result<IEnumerable<NotificationResult>> result = await notificationService.GetUserNotificationsAsync(userId, unreadOnly, page, pageSize);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value.Select(MapToDto));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        Guid userId = GetCurrentUserId();

        Result<int> result = await notificationService.GetUnreadCountAsync(userId);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        Guid userId = GetCurrentUserId();

        Result result = await notificationService.MarkAsReadAsync(id, userId);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }

        return userId;
    }

    private static NotificationDto MapToDto(NotificationResult notification)
    {
        return new NotificationDto(
            notification.Id,
            notification.Type,
            notification.Title,
            notification.Content,
            notification.Metadata,
            notification.IsRead,
            notification.CreatedAt);
    }

    #endregion

}
