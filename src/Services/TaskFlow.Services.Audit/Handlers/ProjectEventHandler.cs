namespace TaskFlow.Services.Audit.Handlers;

using System.Text.Json;

using Application.Services;

using Domain;

using RabbitMQ.Module.Contracts;

using Shared.Messaging.Events;

public class ProjectCreatedHandler(IAuditService auditService, ILogger<ProjectCreatedHandler> logger)
    : BaseAuditHandler(auditService, logger), IMessageHandler<ProjectCreatedEvent>
{

    #region Methods

    public async Task HandleAsync(ProjectCreatedEvent message, IMessageContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Audit: Project created {ProjectId}", message.ProjectId);

        await LogAuditAsync(
            message.OwnerId,
            null,
            AuditAction.Create,
            EntityType.Project,
            message.ProjectId.ToString(),
            null,
            JsonSerializer.Serialize(message),
            context,
            cancellationToken);
    }

    #endregion

}

public class UserAddedToProjectHandler(IAuditService auditService, ILogger<UserAddedToProjectHandler> logger)
    : BaseAuditHandler(auditService, logger), IMessageHandler<UserAddedToProjectEvent>
{

    #region Methods

    public async Task HandleAsync(UserAddedToProjectEvent message, IMessageContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Audit: User {UserId} added to project {ProjectId}", message.UserId, message.ProjectId);

        await LogAuditAsync(
            message.AddedBy,
            null,
            AuditAction.AddMember,
            EntityType.Project,
            message.ProjectId.ToString(),
            null,
            JsonSerializer.Serialize(message),
            context,
            cancellationToken);
    }

    #endregion

}
