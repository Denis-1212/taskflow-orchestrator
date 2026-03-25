namespace TaskFlow.Services.Audit.Handlers;

using System.Text.Json;

using Application.Services;

using RabbitMQ.Module.Contracts;

using Shared.Messaging.Events;

public class UserRegisteredHandler(IAuditService auditService, ILogger<UserRegisteredHandler> logger)
    : BaseAuditHandler(auditService, logger), IMessageHandler<UserRegisteredEvent>
{

    #region Methods

    public async Task HandleAsync(UserRegisteredEvent message, IMessageContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Audit: User registered {UserId}", message.UserId);

        await LogAuditAsync(
            message.UserId,
            message.Email,
            "CREATE",
            "User",
            message.UserId.ToString(),
            null,
            JsonSerializer.Serialize(message),
            context,
            cancellationToken);
    }

    #endregion

}
