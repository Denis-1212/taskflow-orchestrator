namespace TaskFlow.Services.Notification.Extentions;

using Handlers;

using RabbitMQ.Module;
using RabbitMQ.Module.Deduplication;

using Shared.Messaging.Events;

public static class RabbitMQExtensions
{

    #region Methods

    public static IServiceCollection AddRabbitMQModuleWithHandlers(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<TaskCreatedHandler>();
        services.AddScoped<TaskAssignedHandler>();
        services.AddScoped<TaskStatusChangedHandler>();

        // Регистрируем модуль через фабрику, которая получит реальный ServiceProvider
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
                c.QueueName = "notification.task-created";
                // c.ExchangeName = "taskflow.events";
                // c.RoutingKey = "task.created";
                c.PrefetchCount = 10;
            });

            module.AddConsumer<TaskAssignedEvent, TaskAssignedHandler>(c =>
            {
                c.QueueName = "notification.task-assigned-changed";
                // c.ExchangeName = "taskflow.events";
                // c.RoutingKey = "task.assigned";
                c.PrefetchCount = 10;
            });

            module.AddConsumer<TaskStatusChangedEvent, TaskStatusChangedHandler>(c =>
            {
                c.QueueName = "notification.task-status-changed";
                // c.ExchangeName = "taskflow.events";
                // c.RoutingKey = "task.status.changed";
                c.PrefetchCount = 10;
            });

            module.AddConsumer<TaskDeletedEvent, TaskDeletedHandler>(c =>
            {
                c.QueueName = "notification.task-deleted";
                // c.ExchangeName = "taskflow.events";
                // c.RoutingKey = "task.deleted";
                c.PrefetchCount = 10;
            });

            return module;
        });

        return services;
    }

    #endregion

}
