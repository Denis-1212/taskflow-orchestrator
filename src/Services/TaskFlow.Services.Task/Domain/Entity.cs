namespace TaskFlow.Services.Task.Domain;

using Shared.Messaging.Events;

public abstract class Entity
{

    #region Fields

    private readonly List<ITaskEvent> _domainEvents = [];

    #endregion

    #region Properties

    public IReadOnlyCollection<ITaskEvent> DomainEvents => _domainEvents.AsReadOnly();

    #endregion

    #region Methods

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    protected void AddDomainEvent(ITaskEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    #endregion

}
