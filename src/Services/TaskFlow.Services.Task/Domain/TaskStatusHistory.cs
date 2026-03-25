namespace TaskFlow.Services.Task.Domain;

public class TaskStatusHistory
{

    #region Properties

    public Guid Id { get; private set; }
    public Guid TaskId { get; private set; }
    public TaskStatus OldStatus { get; private set; }
    public TaskStatus NewStatus { get; private set; }
    public Guid ChangedBy { get; private set; }
    public DateTime ChangedAt { get; private set; }
    public string Comment { get; private set; }

    #endregion

    #region Constructors

    public TaskStatusHistory(Guid taskId, TaskStatus oldStatus, TaskStatus newStatus, Guid changedBy, string comment)
    {
        Id = Guid.NewGuid();
        TaskId = taskId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        ChangedBy = changedBy;
        ChangedAt = DateTime.UtcNow;
        Comment = comment;
    }

    private TaskStatusHistory()
    {
        Comment = string.Empty;
    }

    #endregion

}
