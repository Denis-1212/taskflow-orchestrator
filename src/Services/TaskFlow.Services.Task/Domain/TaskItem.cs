namespace TaskFlow.Services.Task.Domain;

using Shared.Kernel;

public class TaskItem
{

    #region Fields

    private readonly List<TaskStatusHistory> _statusHistory = new();

    #endregion

    #region Properties

    public Guid Id { get; }
    public Guid ProjectId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public TaskItemStatus Status { get; private set; }
    public TaskPriority Priority { get; private set; }
    public Guid? AssigneeId { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public IReadOnlyCollection<TaskStatusHistory> StatusHistory => _statusHistory.AsReadOnly();

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

        _statusHistory.Add(new TaskStatusHistory(Id, TaskItemStatus.Todo, TaskItemStatus.Todo, createdBy, "Task created"));
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
        Title = title;
        Description = description;
        Priority = priority;
        DueDate = dueDate.Kind == DateTimeKind.Utc ? dueDate : DateTime.SpecifyKind(dueDate, DateTimeKind.Utc);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result AssignTo(Guid assigneeId, Guid assignedBy)
    {
        if (AssigneeId == assigneeId)
        {
            return Result.Success();
        }

        Guid? oldAssignee = AssigneeId;
        AssigneeId = assigneeId;
        UpdatedAt = DateTime.UtcNow;

        _statusHistory.Add(
            new TaskStatusHistory(
                Id,
                Status,
                Status,
                assignedBy,
                $"Assigned to user {assigneeId}"));

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

        // _statusHistory.Add(
        //     new TaskStatusHistory(
        //         Id,
        //         oldStatus,
        //         newStatus,
        //         changedBy,
        //         comment ?? $"Status changed from {oldStatus} to {newStatus}"));

        return Result.Success();
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    #endregion

}
