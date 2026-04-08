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

    #region Constants

    private const string EXCHANGE_NAME = "taskflow.events";
    private const string TASK_CREATED_ROUTING_KEY = "task.created";
    private const string TASK_STATUS_CHANGED_ROUTING_KEY = "task.status.changed";
    private const string TASK_DELETED_ROUTING_KEY = "task.deleted";
    private const string TASK_ASSIGNED_CHANGED_ROUTING_KEY = "task.assigned";
    private const string TASK_UPDATED_ROUTING_KEY = "task.updated";

    #endregion

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

        try
        {
            foreach (OutboxMessage message in messages)
            {
                try
                {
                    await PublishMessageAsync(message, cancellationToken);

                    message.MarkProcessed();

                    logger.LogInformation(
                        "Published outbox message {MessageId} of type {EventType}",
                        message.Id,
                        message.EventType);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to publish outbox message {MessageId}", message.Id);

                    message.IncrementRetry(ex.Message);

                    if (!message.CanRetry(_maxRetryCount))
                    {
                        logger.LogWarning(
                            "Outbox message {MessageId} reached max retry count, moving to dead letter",
                            message.Id);
                    }
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save outbox message states");
            throw;
        }
    }

    private async Task PublishMessageAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
        string eventTypeName = message.EventType;

        var eventType = Type.GetType($"TaskFlow.Shared.Messaging.Events.{eventTypeName}, TaskFlow.Shared.Messaging");

        if (eventType == null)
        {
            throw new InvalidOperationException($"Cannot find type for event: {eventTypeName}");
        }

        object? eventObject = JsonSerializer.Deserialize(message.EventData, eventType);

        if (eventObject == null)
        {
            throw new InvalidOperationException($"Unknown event type: {eventTypeName}");
        }

        switch (eventObject)
        {
            case TaskCreatedEvent e:
                await publisher.PublishAsync(
                    e,
                    o =>
                    {
                        o.WithExchange(EXCHANGE_NAME);
                        o.WithRoutingKey(TASK_CREATED_ROUTING_KEY);
                    },
                    cancellationToken);

                break;
            case TaskAssignedEvent e:
                await publisher.PublishAsync(
                    e,
                    o =>
                    {
                        o.WithExchange(EXCHANGE_NAME);
                        o.WithRoutingKey(TASK_ASSIGNED_CHANGED_ROUTING_KEY);
                    },
                    cancellationToken);

                break;
            case TaskStatusChangedEvent e:
                await publisher.PublishAsync(
                    e,
                    o =>
                    {
                        o.WithExchange(EXCHANGE_NAME);
                        o.WithRoutingKey(TASK_STATUS_CHANGED_ROUTING_KEY);
                    },
                    cancellationToken);

                break;
            case TaskDeletedEvent e:
                await publisher.PublishAsync(
                    e,
                    o =>
                    {
                        o.WithExchange(EXCHANGE_NAME);
                        o.WithRoutingKey(TASK_DELETED_ROUTING_KEY);
                    },
                    cancellationToken);

                break;
            case TaskUpdatedEvent e:
                await publisher.PublishAsync(
                    e,
                    o =>
                    {
                        o.WithExchange(EXCHANGE_NAME);
                        o.WithRoutingKey(TASK_UPDATED_ROUTING_KEY);
                    },
                    cancellationToken);

                break;
        }
    }

    #endregion

}
