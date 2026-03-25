namespace TaskFlow.Shared.DTOs;

public record AuditLogDto(
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
