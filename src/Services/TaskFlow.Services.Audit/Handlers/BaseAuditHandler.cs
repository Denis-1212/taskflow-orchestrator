namespace TaskFlow.Services.Audit.Handlers;

using Application.Services;

using RabbitMQ.Module.Contracts;

public abstract class BaseAuditHandler(IAuditService auditService, ILogger logger)
{

    #region Fields

    protected readonly IAuditService _auditService = auditService;
    protected readonly ILogger _logger = logger;

    #endregion

    #region Methods

    protected async Task LogAuditAsync(
        Guid? userId,
        string? userEmail,
        string action,
        string entityType,
        string entityId,
        string? oldValue,
        string? newValue,
        IMessageContext context,
        CancellationToken cancellationToken)
    {
        // IP and UserAgent would come from message metadata
        // For now, use defaults
        string? ipAddress = context.RoutingKey ?? "unknown";
        string userAgent = "system";

        await _auditService.LogAsync(
            userId,
            userEmail ?? string.Empty,
            action,
            entityType,
            entityId,
            oldValue,
            newValue,
            ipAddress,
            userAgent);

        await context.AckAsync(cancellationToken);
    }

    #endregion

}
