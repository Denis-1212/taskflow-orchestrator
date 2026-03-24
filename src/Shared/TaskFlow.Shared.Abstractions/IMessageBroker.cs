namespace TaskFlow.Shared.Abstractions;

public interface IMessageBroker
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : IEvent;
}
