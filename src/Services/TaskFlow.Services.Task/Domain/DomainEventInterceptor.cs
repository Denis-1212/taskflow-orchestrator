namespace TaskFlow.Services.Task.Domain;

using System.Text.Json;

using Infrastructure;

using Microsoft.EntityFrameworkCore.Diagnostics;

using Shared.Messaging.Events;

using Task = System.Threading.Tasks.Task;

public class DomainEventInterceptor : SaveChangesInterceptor
{

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

        if (outboxMessages.Count != 0)
        {
            await dbContext.OutboxMessages.AddRangeAsync(outboxMessages);
        }
    }

    #endregion

}
