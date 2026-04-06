namespace TaskFlow.Services.Audit.Application.Services;

using Domain;

using Shared.Kernel;

public interface IAuditService
{

    #region Methods

    Task<Result> LogAsync(
        Guid? userId,
        string userEmail,
        AuditAction action,
        EntityType entityType,
        string entityId,
        string? oldValue,
        string? newValue,
        string ipAddress,
        string userAgent,
        CancellationToken token);

    Task<Result<IEnumerable<AuditLogResult>>> SearchAsync(
        Guid? userId,
        string? action,
        string? entityType,
        string? entityId,
        DateTime? from,
        DateTime? to,
        int page = 1,
        int pageSize = 20);

    Task<Result> CleanupOldLogsAsync(int retentionDays);

    #endregion

}

public record AuditLogResult(
    Guid Id,
    Guid? UserId,
    string UserEmail,
    string Action,
    string EntityType,
    string EntityId,
    string? OldValue,
    string? NewValue,
    string IpAddress,
    string UserAgent,
    DateTime Timestamp);
