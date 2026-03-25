namespace TaskFlow.Shared.Messaging.Events;

public record ProjectCreatedEvent : IEvent
{

    #region Properties

    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    public required Guid ProjectId { get; init; }
    public required string ProjectName { get; init; }
    public required Guid OwnerId { get; init; }

    #endregion

}

public record UserAddedToProjectEvent : IEvent
{

    #region Properties

    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    public required Guid ProjectId { get; init; }
    public required string ProjectName { get; init; }
    public required Guid UserId { get; init; }
    public required string UserEmail { get; init; }
    public required string Role { get; init; }
    public required Guid AddedBy { get; init; }

    #endregion

}

public record UserRemovedFromProjectEvent : IEvent
{

    #region Properties

    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    public required Guid ProjectId { get; init; }
    public required string ProjectName { get; init; }
    public required Guid UserId { get; init; }
    public required string UserEmail { get; init; }
    public required Guid RemovedBy { get; init; }

    #endregion

}
