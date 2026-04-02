namespace TaskFlow.Services.Task.Extensions;

using RabbitMQ.Module;
using RabbitMQ.Module.Deduplication;

public static class RabbitMQExtensions
{

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
            // configureHandlers(module);

            return module;
        });

        // Регистрируем Publisher как Singleton
        services.AddSingleton(sp => sp.GetRequiredService<MessagingModule>().CreatePublisher());

        return services;
    }

    #endregion

}
