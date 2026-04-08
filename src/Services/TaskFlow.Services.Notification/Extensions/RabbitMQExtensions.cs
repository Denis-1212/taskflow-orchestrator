namespace TaskFlow.Services.Notification.Extensions;

using Handlers;

using RabbitMQ.Module;
using RabbitMQ.Module.Deduplication;

using Shared.Messaging.Events;

public static class RabbitMQExtensions
{

    #region Constants

    private const string EXCHANGE_NAME = "taskflow.events";

    private const string TASK_CREATED_QUEUE_NAME = "notification.task-created";
    private const string TASK_ASSIGNED_CHANGED_QUEUE_NAME = "notification.task-assigned-changed";
    private const string TASK_STATUS_CHANGED_QUEUE_NAME = "notification.task-status-changed";
    private const string TASK_DELETED_QUEUE_NAME = "notification.task-deleted";
    private const string TASK_UPDATED_QUEUE_NAME = "notification.task-updated";

    private const string TASK_CREATED_ROUTING_KEY = "task.created";
    private const string TASK_STATUS_CHANGED_ROUTING_KEY = "task.status.changed";
    private const string TASK_DELETED_ROUTING_KEY = "task.deleted";
    private const string TASK_ASSIGNED_CHANGED_ROUTING_KEY = "task.assigned";
    private const string TASK_UPDATED_ROUTING_KEY = "task.updated";

    #endregion

    #region Methods

    public static IServiceCollection AddRabbitMQModuleWithHandlers(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            var module = MessagingModule.Create(
                options =>
                {
                    IConfigurationSection rabbitMqConfig = configuration.GetSection("RabbitMQ");
                    options.ConnectionString =
                        $"amqp://{rabbitMqConfig["Username"]}:{rabbitMqConfig["Password"]}@{rabbitMqConfig["Host"]}:{rabbitMqConfig["Port"]}{rabbitMqConfig["VirtualHost"]}";

                    options.ClientProvidedName = configuration["RabbitMQ:ClientProvidedName"] ?? "TaskFlow";
                    options.DeliveryControl.PublisherConfirmsEnabled = true;
                    options.DeliveryControl.EnableDeadLetter = true;
                    options.DeliveryControl.MaxRetryAttempts = 3;
                    options.DeliveryControl.RetryBaseDelayMs = 1000;
                    options.Deduplication.StoreType = DeduplicationStoreType.InMemory;
                },
                loggerFactory,
                sp);

            // Регистрируем потребителей
            module.AddConsumer<TaskCreatedEvent, TaskCreatedHandler>(c =>
            {
                c.QueueName = TASK_CREATED_QUEUE_NAME;
                c.ExchangeName = EXCHANGE_NAME;
                c.RoutingKey = TASK_CREATED_ROUTING_KEY;
                c.PrefetchCount = 10;
            });

            module.AddConsumer<TaskAssignedEvent, TaskAssignedHandler>(c =>
            {
                c.QueueName = TASK_ASSIGNED_CHANGED_QUEUE_NAME;
                c.ExchangeName = EXCHANGE_NAME;
                c.RoutingKey = TASK_ASSIGNED_CHANGED_ROUTING_KEY;
                c.PrefetchCount = 10;
            });

            module.AddConsumer<TaskStatusChangedEvent, TaskStatusChangedHandler>(c =>
            {
                c.QueueName = TASK_STATUS_CHANGED_QUEUE_NAME;
                c.ExchangeName = EXCHANGE_NAME;
                c.RoutingKey = TASK_STATUS_CHANGED_ROUTING_KEY;
                c.PrefetchCount = 10;
            });

            module.AddConsumer<TaskDeletedEvent, TaskDeletedHandler>(c =>
            {
                c.QueueName = TASK_DELETED_QUEUE_NAME;
                c.ExchangeName = EXCHANGE_NAME;
                c.RoutingKey = TASK_DELETED_ROUTING_KEY;
                c.PrefetchCount = 10;
            });

            module.AddConsumer<TaskUpdatedEvent, TaskUpdatedHandler>(c =>
            {
                c.QueueName = TASK_UPDATED_QUEUE_NAME;
                c.ExchangeName = EXCHANGE_NAME;
                c.RoutingKey = TASK_UPDATED_ROUTING_KEY;
                c.PrefetchCount = 10;
            });

            return module;
        });

        return services;
    }

    #endregion

}
