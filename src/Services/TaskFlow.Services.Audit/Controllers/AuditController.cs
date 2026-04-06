namespace TaskFlow.Services.Audit.Controllers;

using Application.Services;

using Microsoft.AspNetCore.Mvc;

using Shared.DTOs;
using Shared.Kernel;

[ApiController]
[Route("api/audit")]
public class AuditController(IAuditService auditService) : ControllerBase
{

    #region Methods

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> Search(
        [FromQuery] Guid? userId,
        [FromQuery] string? action,
        [FromQuery] string? entityType,
        [FromQuery] string? entityId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        Result<IEnumerable<AuditLogResult>> result = await auditService.SearchAsync(userId, action, entityType, entityId, from, to, page, pageSize);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value.Select(MapToDto));
    }

    [HttpPost("cleanup")]
    public async Task<IActionResult> Cleanup([FromQuery] int retentionDays = 90)
    {
        Result result = await auditService.CleanupOldLogsAsync(retentionDays);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(
            new
            {
                message = $"Cleaned up logs older than {retentionDays} days"
            });
    }

    private static AuditLogDto MapToDto(AuditLogResult log)
    {
        return new AuditLogDto(
            log.Id,
            log.UserId,
            log.UserEmail,
            log.Action,
            log.EntityType,
            log.EntityId,
            log.OldValue,
            log.NewValue,
            log.IpAddress,
            log.UserAgent,
            log.Timestamp);
    }

    #endregion

}
