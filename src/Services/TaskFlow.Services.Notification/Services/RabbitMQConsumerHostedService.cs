namespace TaskFlow.Services.Notification.Services;

using RabbitMQ.Module;

public class RabbitMQConsumerHostedService : BackgroundService
{

    #region Fields

    private readonly MessagingModule _messagingModule;
    private readonly ILogger<RabbitMQConsumerHostedService> _logger;

    #endregion

    #region Constructors

    public RabbitMQConsumerHostedService(ILogger<RabbitMQConsumerHostedService> logger)
    {
        _logger = logger;
        _messagingModule = MessagingModule.Create(options =>
        {
            // Options will be loaded from configuration
        });
    }

    #endregion

    #region Methods

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping RabbitMQ consumers...");

        await _messagingModule.StopConsumersAsync(cancellationToken);

        _logger.LogInformation("RabbitMQ consumers stopped");

        await base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting RabbitMQ consumers...");

        await _messagingModule.StartConsumersAsync(stoppingToken);

        _logger.LogInformation("RabbitMQ consumers started");
    }

    #endregion

}
