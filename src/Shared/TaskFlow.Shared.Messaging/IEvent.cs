namespace TaskFlow.Shared.Messaging;

public interface IEvent : TaskFlow.Shared.Abstractions.IEvent
{
}

public abstract class EventBase : IEvent
{
    public Guid EventId { get; protected set; } = Guid.NewGuid();
    public DateTime OccurredAt { get; protected set; } = DateTime.UtcNow;
}
