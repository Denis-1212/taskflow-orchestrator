using TaskFlow.Shared.Messaging;

public abstract class EventBase : IEvent
{

    #region Properties

    public Guid EventId { get; protected set; } = Guid.NewGuid();
    public DateTime OccurredAt { get; protected set; } = DateTime.UtcNow;

    #endregion

}
