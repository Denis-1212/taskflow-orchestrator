namespace TaskFlow.Services.Task.Application.Services;

using System.Text.Json;

using Clients;

using Domain;

using Infrastructure;

using Microsoft.EntityFrameworkCore;

using Shared.Kernel;
using Shared.Messaging.Events;

public class TaskService(
    TaskDbContext context,
    IProjectGrpcClient projectClient,
    ILogger<TaskService> logger)
    : ITaskService
{

    #region Methods

    public async Task<Result<TaskResult>> CreateAsync(
        Guid projectId,
        string title,
        string description,
        string priority,
        Guid? assigneeId,
        Guid createdBy,
        DateTime dueDate)
    {
        logger.LogInformation("Creating task in project {ProjectId} by user {CreatedBy}", projectId, createdBy);

        // Validate project exists
        bool projectExists = await projectClient.ProjectExistsAsync(projectId);

        if (!projectExists)
        {
            return Result.Failure<TaskResult>(Error.NotFound("Project", projectId));
        }

        // Validate assignee if provided
        if (assigneeId.HasValue)
        {
            (bool IsMember, string Role) memberValidation = await projectClient.ValidateMemberAsync(projectId, assigneeId.Value);

            if (!memberValidation.IsMember)
            {
                return Result.Failure<TaskResult>(Error.Validation($"User {assigneeId} is not a member of project {projectId}"));
            }
        }

        if (!Enum.TryParse(priority, true, out TaskPriority taskPriority))
        {
            return Result.Failure<TaskResult>(Error.Validation($"Invalid priority: {priority}"));
        }

        var task = new TaskItem(projectId, title, description, taskPriority, assigneeId, createdBy, dueDate);

        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        // Save outbox message
        var taskCreatedEvent = new TaskCreatedEvent
        {
            TaskId = task.Id,
            ProjectId = task.ProjectId,
            TaskTitle = task.Title,
            AssigneeId = task.AssigneeId,
            CreatedBy = task.CreatedBy,
            DueDate = task.DueDate,
            Priority = task.Priority.ToString()
        };

        var outboxMessage = new OutboxMessage(
            nameof(TaskCreatedEvent),
            JsonSerializer.Serialize(taskCreatedEvent));

        context.OutboxMessages.Add(outboxMessage);
        await context.SaveChangesAsync();

        logger.LogInformation("Task created with Id: {TaskId}", task.Id);

        return MapToResult(task);
    }

    public async Task<Result<TaskResult>> UpdateAsync(
        Guid taskId,
        string title,
        string description,
        string priority,
        DateTime dueDate,
        Guid userId)
    {
        logger.LogInformation("Updating task {TaskId} by user {UserId}", taskId, userId);

        TaskItem? task = await context.Tasks
                             .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);

        if (task == null)
        {
            return Result.Failure<TaskResult>(Error.NotFound("Task", taskId));
        }

        // Validate user is member of project
        (bool IsMember, string Role) memberValidation = await projectClient.ValidateMemberAsync(task.ProjectId, userId);

        if (!memberValidation.IsMember)
        {
            return Result.Failure<TaskResult>(Error.Forbidden("User is not a member of this project"));
        }

        if (!Enum.TryParse(priority, true, out TaskPriority taskPriority))
        {
            return Result.Failure<TaskResult>(Error.Validation($"Invalid priority: {priority}"));
        }

        task.Update(title, description, taskPriority, dueDate, userId);
        await context.SaveChangesAsync();

        return MapToResult(task);
    }

    public async Task<Result> DeleteAsync(Guid taskId, Guid userId)
    {
        logger.LogInformation("Deleting task {TaskId} by user {UserId}", taskId, userId);

        TaskItem? task = await context.Tasks
                             .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);

        if (task == null)
        {
            return Result.Failure(Error.NotFound("Task", taskId));
        }

        // Validate user is member of project
        (bool IsMember, string Role) memberValidation = await projectClient.ValidateMemberAsync(task.ProjectId, userId);

        if (!memberValidation.IsMember)
        {
            return Result.Failure(Error.Forbidden("User is not a member of this project"));
        }

        task.SoftDelete();
        await context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<TaskResult>> GetByIdAsync(Guid taskId, Guid userId)
    {
        TaskItem? task = await context.Tasks
                             .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);

        if (task == null)
        {
            return Result.Failure<TaskResult>(Error.NotFound("Task", taskId));
        }

        // Validate user is member of project
        (bool IsMember, string Role) memberValidation = await projectClient.ValidateMemberAsync(task.ProjectId, userId);

        if (!memberValidation.IsMember)
        {
            return Result.Failure<TaskResult>(Error.Forbidden("User is not a member of this project"));
        }

        return MapToResult(task);
    }

    public async Task<Result<IEnumerable<TaskResult>>> GetTasksAsync(
        Guid? projectId,
        string? status,
        string? priority,
        Guid? assigneeId,
        Guid userId)
    {
        IQueryable<TaskItem>? query = context.Tasks.AsQueryable();

        if (projectId.HasValue)
        {
            // Validate user is member of project
            (bool IsMember, string Role) memberValidation = await projectClient.ValidateMemberAsync(projectId.Value, userId);

            if (!memberValidation.IsMember)
            {
                return Result.Failure<IEnumerable<TaskResult>>(Error.Forbidden("User is not a member of this project"));
            }

            query = query.Where(t => t.ProjectId == projectId.Value);
        }

        if (!string.IsNullOrEmpty(status) && Enum.TryParse(status, true, out TaskStatus taskStatus))
        {
            query = query.Where(t => t.Status == taskStatus);
        }

        if (!string.IsNullOrEmpty(priority) && Enum.TryParse(priority, true, out TaskPriority taskPriority))
        {
            query = query.Where(t => t.Priority == taskPriority);
        }

        if (assigneeId.HasValue)
        {
            query = query.Where(t => t.AssigneeId == assigneeId.Value);
        }

        query = query.Where(t => !t.IsDeleted);

        List<TaskItem> tasks = await query.ToListAsync();

        return tasks.Select(MapToResult).ToList();
    }

    public async Task<Result<TaskResult>> AssignTaskAsync(Guid taskId, Guid assigneeId, Guid assignedBy)
    {
        logger.LogInformation(
            "Assigning task {TaskId} to user {AssigneeId} by {AssignedBy}",
            taskId,
            assigneeId,
            assignedBy);

        TaskItem? task = await context.Tasks
                             .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);

        if (task == null)
        {
            return Result.Failure<TaskResult>(Error.NotFound("Task", taskId));
        }

        // Validate assignee is member of project
        (bool IsMember, string Role) memberValidation = await projectClient.ValidateMemberAsync(task.ProjectId, assigneeId);

        if (!memberValidation.IsMember)
        {
            return Result.Failure<TaskResult>(Error.Validation($"User {assigneeId} is not a member of project {task.ProjectId}"));
        }

        task.AssignTo(assigneeId, assignedBy);
        await context.SaveChangesAsync();

        // Save outbox message
        var taskAssignedEvent = new TaskAssignedEvent
        {
            TaskId = task.Id,
            TaskTitle = task.Title,
            ProjectId = task.ProjectId,
            ProjectName = string.Empty,
            AssigneeId = assigneeId,
            AssigneeEmail = string.Empty,
            AssignedBy = assignedBy,
            DueDate = task.DueDate
        };

        var outboxMessage = new OutboxMessage(
            nameof(TaskAssignedEvent),
            JsonSerializer.Serialize(taskAssignedEvent));

        context.OutboxMessages.Add(outboxMessage);
        await context.SaveChangesAsync();

        return MapToResult(task);
    }

    public async Task<Result<TaskResult>> ChangeStatusAsync(Guid taskId, string status, Guid changedBy, string? comment)
    {
        logger.LogInformation(
            "Changing status of task {TaskId} to {Status} by {ChangedBy}",
            taskId,
            status,
            changedBy);

        if (!Enum.TryParse(status, true, out TaskStatus newStatus))
        {
            return Result.Failure<TaskResult>(Error.Validation($"Invalid status: {status}"));
        }

        TaskItem? task = await context.Tasks
                             .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);

        if (task == null)
        {
            return Result.Failure<TaskResult>(Error.NotFound("Task", taskId));
        }

        // Validate user is member of project
        (bool IsMember, string Role) memberValidation = await projectClient.ValidateMemberAsync(task.ProjectId, changedBy);

        if (!memberValidation.IsMember)
        {
            return Result.Failure<TaskResult>(Error.Forbidden("User is not a member of this project"));
        }

        string oldStatus = task.Status.ToString();
        task.ChangeStatus(newStatus, changedBy, comment);
        await context.SaveChangesAsync();

        // Save outbox message
        var statusChangedEvent = new TaskStatusChangedEvent
        {
            TaskId = task.Id,
            TaskTitle = task.Title,
            ProjectId = task.ProjectId,
            OldStatus = oldStatus,
            NewStatus = status,
            ChangedBy = changedBy,
            ChangedByEmail = string.Empty
        };

        var outboxMessage = new OutboxMessage(
            nameof(TaskStatusChangedEvent),
            JsonSerializer.Serialize(statusChangedEvent));

        context.OutboxMessages.Add(outboxMessage);
        await context.SaveChangesAsync();

        return MapToResult(task);
    }

    public async Task<Result<TaskStatisticsResult>> GetStatisticsAsync(Guid projectId, Guid userId)
    {
        // Validate user is member of project
        (bool IsMember, string Role) memberValidation = await projectClient.ValidateMemberAsync(projectId, userId);

        if (!memberValidation.IsMember)
        {
            return Result.Failure<TaskStatisticsResult>(Error.Forbidden("User is not a member of this project"));
        }

        List<TaskItem> tasks = await context.Tasks
                                   .Where(t => t.ProjectId == projectId && !t.IsDeleted)
                                   .ToListAsync();

        var statistics = new TaskStatisticsResult(
            tasks.Count,
            tasks.Count(t => t.Status == TaskStatus.Todo),
            tasks.Count(t => t.Status == TaskStatus.InProgress),
            tasks.Count(t => t.Status == TaskStatus.Completed),
            tasks.Count(t => t.Status == TaskStatus.Cancelled));

        return statistics;
    }

    private static TaskResult MapToResult(TaskItem task)
    {
        return new TaskResult(
            task.Id,
            task.ProjectId,
            task.Title,
            task.Description,
            task.Status.ToString(),
            task.Priority.ToString(),
            task.AssigneeId,
            task.DueDate,
            task.CreatedAt);
    }

    #endregion

}
