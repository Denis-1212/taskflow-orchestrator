namespace TaskFlow.Shared.Messaging.Events;

public record UserRegisteredEvent : IEvent
{

    #region Properties

    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required string FullName { get; init; }

    #endregion

}

public record UserLoggedInEvent : IEvent
{

    #region Properties

    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required string IpAddress { get; init; }

    #endregion

}

public record UserLoggedOutEvent : IEvent
{

    #region Properties

    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    public required Guid UserId { get; init; }
    public required string Email { get; init; }

    #endregion

}
