namespace TaskFlow.Shared.DTOs;

public record NotificationDto(
    Guid Id,
    string Type,
    string Title,
    string Content,
    string Metadata,
    bool IsRead,
    DateTime CreatedAt);
