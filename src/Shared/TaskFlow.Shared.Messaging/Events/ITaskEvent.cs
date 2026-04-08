namespace TaskFlow.Shared.Messaging.Events;

public interface ITaskEvent : IEvent
{

    #region Properties

    Guid TaskId { get; }

    #endregion

}
