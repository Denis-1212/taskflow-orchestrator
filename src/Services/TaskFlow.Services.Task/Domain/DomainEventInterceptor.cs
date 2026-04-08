namespace TaskFlow.Services.Task.Domain;

using System.Text.Json;

using Infrastructure;

using Microsoft.EntityFrameworkCore.Diagnostics;

using RabbitMQ.Module.Contracts;

using Shared.Messaging.Events;

using Task = System.Threading.Tasks.Task;

public class DomainEventInterceptor(IPublisher publisher) : SaveChangesInterceptor
{

    #region Constants

    private const string EXCHANGE_NAME = "taskflow.events";
    private const string TASK_CREATED_ROUTING_KEY = "task.created";
    private const string TASK_STATUS_CHANGED_ROUTING_KEY = "task.status.changed";
    private const string TASK_DELETED_ROUTING_KEY = "task.deleted";
    private const string TASK_ASSIGNED_CHANGED_ROUTING_KEY = "task.assigned";
    private const string TASK_UPDATED_ROUTING_KEY = "task.updated";

    #endregion

    #region Methods

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not TaskDbContext dbContext)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        List<ITaskEvent> domainEvents = dbContext.ChangeTracker
            .Entries<AggregateRoot>()
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        if (domainEvents.Count == 0)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        await ProcessStatusHistory(dbContext, domainEvents);
        await ProcessOutboxMessages(dbContext, domainEvents);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private async Task ProcessStatusHistory(TaskDbContext dbContext, List<ITaskEvent> events)
    {
        IEnumerable<TaskStatusChangedEvent> statusChanges = events.OfType<TaskStatusChangedEvent>();

        List<TaskStatusHistory> histories = statusChanges.Select(e => new TaskStatusHistory(
            e.TaskId,
            Enum.Parse<TaskItemStatus>(e.OldStatus),
            Enum.Parse<TaskItemStatus>(e.NewStatus),
            e.ChangedBy,
            string.Empty
        )).ToList();

        if (histories.Count != 0)
        {
            await dbContext.TaskStatusHistories.AddRangeAsync(histories);
        }
    }

    private async Task ProcessOutboxMessages(TaskDbContext dbContext, List<ITaskEvent> events)
    {
        List<OutboxMessage> outboxMessages = events.Select(e => new OutboxMessage(
            e.TaskId,
            e.GetType().Name,
            JsonSerializer.Serialize(e, e.GetType())
        )).ToList();

        // var taskUpdatedEvent = new TaskUpdatedEvent
        // {
        //     TaskId = outboxMessages[0].TaskId,
        //     NewTitle = "Test Updated",
        //     ProjectId = outboxMessages[0].TaskId,
        //     TaskTitle = "Test Updated",
        //     OldTitle = "!!!!!!!!!!!",
        //     UpdatedBy = outboxMessages[0].TaskId
        // };
        //
        // await publisher.PublishAsync(
        //     taskUpdatedEvent,
        //     option =>
        //     {
        //         option.WithExchange(EXCHANGE_NAME);
        //         option.WithRoutingKey(TASK_UPDATED_ROUTING_KEY);
        //     }
        // );

        if (outboxMessages.Count != 0)
        {
            await dbContext.OutboxMessages.AddRangeAsync(outboxMessages);
        }
    }

    #endregion

}
