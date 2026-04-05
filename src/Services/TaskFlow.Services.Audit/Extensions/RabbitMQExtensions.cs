namespace TaskFlow.Services.Audit.Extensions;

using Handlers;

using RabbitMQ.Module;
using RabbitMQ.Module.Deduplication;

using Shared.Messaging.Events;

public static class RabbitMQExtensions
{

    #region Constants

    private const string EXCHANGE_NAME = "taskflow.events";

    private const string TASK_CREATED_QUEUE_NAME = "audit.task-created";
    private const string TASK_ASSIGNED_CHANGED_QUEUE_NAME = "audit.task-assigned-changed";
    private const string TASK_STATUS_CHANGED_QUEUE_NAME = "audit.task-status-changed";
    private const string TASK_DELETED_QUEUE_NAME = "audit.task-deleted";

    private const string PROJECT_CREATED_QUEUE_NAME = "audit.project-created";
    private const string PROJECT_USER_ADDED_QUEUE_NAME = "audit.user-added-to-project";

    private const string USER_REGISTERED_QUEUE_NAME = "audit.user-registered";

    private const string TASK_CREATED_ROUTING_KEY = "task.created";
    private const string TASK_STATUS_CHANGED_ROUTING_KEY = "task.status.changed";
    private const string TASK_DELETED_ROUTING_KEY = " task.deleted";
    private const string TASK_ASSIGNED_CHANGED_ROUTING_KEY = "task.assigned";

    private const string PROJECT_CREATED_ROUTING_KEY = "project.created";
    private const string PROJECT_USER_ADDED_ROUTING_KEY = "project.user.added";

    private const string USER_REGISTERED_ROUTING_KEY = "user.registered";

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

            module.AddConsumer<UserRegisteredEvent, UserRegisteredHandler>(c =>
            {
                c.QueueName = USER_REGISTERED_QUEUE_NAME;
                c.ExchangeName = EXCHANGE_NAME;
                c.RoutingKey = USER_REGISTERED_ROUTING_KEY;
                c.PrefetchCount = 10;
            });

            module.AddConsumer<ProjectCreatedEvent, ProjectCreatedHandler>(c =>
            {
                c.QueueName = PROJECT_CREATED_QUEUE_NAME;
                c.ExchangeName = EXCHANGE_NAME;
                c.RoutingKey = PROJECT_CREATED_ROUTING_KEY;
                c.PrefetchCount = 10;
            });

            module.AddConsumer<UserAddedToProjectEvent, UserAddedToProjectHandler>(c =>
            {
                c.QueueName = PROJECT_USER_ADDED_QUEUE_NAME;
                c.ExchangeName = EXCHANGE_NAME;
                c.RoutingKey = PROJECT_USER_ADDED_ROUTING_KEY;
                c.PrefetchCount = 10;
            });

            return module;
        });

        return services;
    }

    #endregion

}
