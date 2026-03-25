namespace TaskFlow.Services.Task.Application.Services;

using Shared.Kernel;

public interface ITaskService
{

    #region Methods

    Task<Result<TaskResult>> CreateAsync(
        Guid projectId,
        string title,
        string description,
        string priority,
        Guid? assigneeId,
        Guid createdBy,
        DateTime dueDate);

    Task<Result<TaskResult>> UpdateAsync(
        Guid taskId,
        string title,
        string description,
        string priority,
        DateTime dueDate,
        Guid userId);

    Task<Result> DeleteAsync(Guid taskId, Guid userId);

    Task<Result<TaskResult>> GetByIdAsync(Guid taskId, Guid userId);

    Task<Result<IEnumerable<TaskResult>>> GetTasksAsync(
        Guid? projectId,
        string? status,
        string? priority,
        Guid? assigneeId,
        Guid userId);

    Task<Result<TaskResult>> AssignTaskAsync(Guid taskId, Guid assigneeId, Guid assignedBy);

    Task<Result<TaskResult>> ChangeStatusAsync(Guid taskId, string status, Guid changedBy, string? comment);

    Task<Result<TaskStatisticsResult>> GetStatisticsAsync(Guid projectId, Guid userId);

    #endregion

}

public record TaskResult(
    Guid Id,
    Guid ProjectId,
    string Title,
    string Description,
    string Status,
    string Priority,
    Guid? AssigneeId,
    DateTime DueDate,
    DateTime CreatedAt);

public record TaskStatisticsResult(
    int Total,
    int Todo,
    int InProgress,
    int Completed,
    int Cancelled);
