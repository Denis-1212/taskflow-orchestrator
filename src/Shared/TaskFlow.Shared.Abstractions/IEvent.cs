namespace TaskFlow.Shared.Abstractions;

public interface IEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
