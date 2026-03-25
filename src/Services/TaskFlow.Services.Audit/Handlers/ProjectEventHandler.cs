namespace TaskFlow.Services.Audit.Handlers;

using System.Text.Json;

using Application.Services;

using RabbitMQ.Module.Contracts;

using Shared.Messaging.Events;

public class ProjectCreatedHandler : BaseAuditHandler, IMessageHandler<ProjectCreatedEvent>
{

    #region Constructors

    public ProjectCreatedHandler(IAuditService auditService, ILogger<ProjectCreatedHandler> logger)
        : base(auditService, logger)
    {
    }

    #endregion

    #region Methods

    public async Task HandleAsync(ProjectCreatedEvent message, IMessageContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Audit: Project created {ProjectId}", message.ProjectId);

        await LogAuditAsync(
            message.OwnerId,
            null,
            "CREATE",
            "Project",
            message.ProjectId.ToString(),
            null,
            JsonSerializer.Serialize(message),
            context,
            cancellationToken);
    }

    #endregion

}

public class UserAddedToProjectHandler : BaseAuditHandler, IMessageHandler<UserAddedToProjectEvent>
{

    #region Constructors

    public UserAddedToProjectHandler(IAuditService auditService, ILogger<UserAddedToProjectHandler> logger)
        : base(auditService, logger)
    {
    }

    #endregion

    #region Methods

    public async Task HandleAsync(UserAddedToProjectEvent message, IMessageContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Audit: User {UserId} added to project {ProjectId}", message.UserId, message.ProjectId);

        await LogAuditAsync(
            message.AddedBy,
            null,
            "ADD_MEMBER",
            "Project",
            message.ProjectId.ToString(),
            null,
            JsonSerializer.Serialize(message),
            context,
            cancellationToken);
    }

    #endregion

}
