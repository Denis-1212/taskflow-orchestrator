namespace TaskFlow.Shared.Messaging.Events;

public record TaskCreatedEvent : IEvent
{

    #region Properties

    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    public required Guid TaskId { get; init; }
    public required Guid ProjectId { get; init; }
    public required string TaskTitle { get; init; }
    public Guid? AssigneeId { get; init; }
    public string? AssigneeEmail { get; init; }
    public required Guid CreatedBy { get; init; }
    public required DateTime DueDate { get; init; }
    public required string Priority { get; init; }

    #endregion

}

public record TaskAssignedEvent : IEvent
{

    #region Properties

    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    public required Guid TaskId { get; init; }
    public required string TaskTitle { get; init; }
    public required Guid ProjectId { get; init; }
    public required string ProjectName { get; init; }
    public required Guid AssigneeId { get; init; }
    public required string AssigneeEmail { get; init; }
    public required Guid AssignedBy { get; init; }
    public required DateTime DueDate { get; init; }

    #endregion

}

public record TaskStatusChangedEvent : IEvent
{

    #region Properties

    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    public required Guid TaskId { get; init; }
    public required string TaskTitle { get; init; }
    public required Guid ProjectId { get; init; }
    public required string OldStatus { get; init; }
    public required string NewStatus { get; init; }
    public required Guid ChangedBy { get; init; }
    public required string ChangedByEmail { get; init; }

    #endregion

}
