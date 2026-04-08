namespace TaskFlow.Services.Task.Domain;

using Shared.Kernel;
using Shared.Messaging.Events;

public class TaskItem : AggregateRoot
{

    #region Properties

    public Guid Id { get; }
    public Guid ProjectId { get; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public TaskItemStatus Status { get; private set; }
    public TaskPriority Priority { get; private set; }
    public Guid? AssigneeId { get; private set; }
    public Guid CreatedBy { get; }
    public DateTime DueDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }

    #endregion

    #region Constructors

    public TaskItem(
        Guid projectId,
        string title,
        string description,
        TaskPriority priority,
        Guid? assigneeId,
        Guid createdBy,
        DateTime dueDate)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        Title = title;
        Description = description;
        Status = TaskItemStatus.Todo;
        Priority = priority;
        AssigneeId = assigneeId;
        CreatedBy = createdBy;
        DueDate = dueDate.Kind == DateTimeKind.Utc ? dueDate : DateTime.SpecifyKind(dueDate, DateTimeKind.Utc);
        CreatedAt = DateTime.UtcNow;
        IsDeleted = false;

        AddDomainEvent(
            new TaskCreatedEvent
            {
                TaskId = Id,
                ProjectId = ProjectId,
                TaskTitle = Title,
                AssigneeId = AssigneeId,
                CreatedBy = CreatedBy,
                DueDate = DueDate,
                Priority = Priority.ToString()
            });
    }

    private TaskItem()
    {
        Title = string.Empty;
        Description = string.Empty;
    }

    #endregion

    #region Methods

    public Result Update(string title, string description, TaskPriority priority, DateTime dueDate, Guid userId)
    {
        string oldTitle = Title;
        Title = title;
        Description = description;
        Priority = priority;

        DueDate = dueDate.Kind == DateTimeKind.Utc ? dueDate : DateTime.SpecifyKind(dueDate, DateTimeKind.Utc);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(
            new TaskUpdatedEvent
            {
                TaskId = Id,
                TaskTitle = Title,
                ProjectId = ProjectId,
                OldTitle = oldTitle,
                NewTitle = Title,
                UpdatedBy = userId
            });

        return Result.Success();
    }

    public Result AssignTo(Guid assigneeId, Guid assignedBy)
    {
        if (AssigneeId == assigneeId)
        {
            return Result.Success();
        }

        AssigneeId = assigneeId;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(
            new TaskAssignedEvent
            {
                TaskId = Id,
                TaskTitle = Title,
                ProjectId = ProjectId,
                AssigneeId = assigneeId,
                AssignedBy = assignedBy,
                DueDate = DueDate
            });

        return Result.Success();
    }

    public Result ChangeStatus(TaskItemStatus newStatus, Guid changedBy, string? comment = null)
    {
        if (Status == newStatus)
        {
            return Result.Success();
        }

        TaskItemStatus oldStatus = Status;
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(
            new TaskStatusChangedEvent
            {
                TaskId = Id,
                TaskTitle = Title,
                ProjectId = ProjectId,
                OldStatus = oldStatus.ToString(),
                NewStatus = newStatus.ToString(),
                ChangedBy = changedBy,
                ChangedByEmail = string.Empty
            });

        return Result.Success();
    }

    public void SoftDelete(Guid deletedBy)
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(
            new TaskDeletedEvent
            {
                DeletedBy = deletedBy,
                OccurredAt = DateTime.UtcNow,
                ProjectId = ProjectId,
                TaskId = Id,
                TaskTitle = Title
            });
    }

    #endregion

}
