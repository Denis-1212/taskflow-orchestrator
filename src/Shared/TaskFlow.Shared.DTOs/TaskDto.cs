namespace TaskFlow.Shared.DTOs;

public record TaskDto(
    Guid Id,
    Guid ProjectId,
    string Title,
    string Description,
    string Status,
    string Priority,
    Guid? AssigneeId,
    string? AssigneeName,
    DateTime DueDate,
    DateTime CreatedAt);

public record CreateTaskDto(Guid ProjectId, string Title, string Description, string Priority, Guid? AssigneeId, DateTime DueDate);

public record UpdateTaskDto(string Title, string Description, string Priority, DateTime DueDate);

public record AssignTaskDto(Guid AssigneeId);

public record ChangeStatusDto(string Status, string? Comment);

public record TaskStatisticsDto(int Total, int Todo, int InProgress, int Completed, int Cancelled);
