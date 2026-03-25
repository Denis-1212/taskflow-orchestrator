namespace TaskFlow.Services.Task.Services;

using System.Text.Json;

using Domain;

using Infrastructure;

using Microsoft.EntityFrameworkCore;

using RabbitMQ.Module.Contracts;

using Shared.Messaging.Events;

using Task = System.Threading.Tasks.Task;

public class OutboxProcessorService(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxProcessorService> logger)
    : BackgroundService
{

    #region Fields

    private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);
    private readonly int _maxRetryCount = 5;

    #endregion

    #region Methods

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox Processor Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        logger.LogInformation("Outbox Processor Service stopped");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

        List<OutboxMessage> messages = await context.OutboxMessages
                                           .Where(m => m.ProcessedAt == null)
                                           .OrderBy(m => m.CreatedAt)
                                           .Take(100)
                                           .ToListAsync(cancellationToken);

        if (!messages.Any())
        {
            return;
        }

        logger.LogInformation("Processing {Count} outbox messages", messages.Count);

        foreach (OutboxMessage message in messages)
        {
            try
            {
                await PublishMessageAsync(message, publisher, cancellationToken);

                message.MarkProcessed();
                await context.SaveChangesAsync(cancellationToken);

                logger.LogDebug(
                    "Published outbox message {MessageId} of type {EventType}",
                    message.Id,
                    message.EventType);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to publish outbox message {MessageId}", message.Id);

                message.IncrementRetry(ex.Message);
                await context.SaveChangesAsync(cancellationToken);

                if (!message.CanRetry(_maxRetryCount))
                {
                    logger.LogWarning(
                        "Outbox message {MessageId} reached max retry count, moving to dead letter",
                        message.Id);
                    // Optionally move to dead letter table or mark with error
                }
            }
        }
    }

    private async Task PublishMessageAsync(
        OutboxMessage message,
        IPublisher publisher,
        CancellationToken cancellationToken)
    {
        // Determine event type from stored type name
        string eventTypeName = message.EventType;

        // Deserialize to object based on type
        object? eventObject = eventTypeName switch
        {
            nameof(TaskCreatedEvent) => JsonSerializer.Deserialize<TaskCreatedEvent>(message.EventData),
            nameof(TaskAssignedEvent) => JsonSerializer.Deserialize<TaskAssignedEvent>(message.EventData),
            nameof(TaskStatusChangedEvent) => JsonSerializer.Deserialize<TaskStatusChangedEvent>(message.EventData),
            _ => null
        };

        if (eventObject == null)
        {
            throw new InvalidOperationException($"Unknown event type: {eventTypeName}");
        }

        // Publish to RabbitMQ
        await publisher.PublishAsync(eventObject, cancellationToken: cancellationToken);
    }

    #endregion

}
