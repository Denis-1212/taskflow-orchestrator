namespace TaskFlow.Services.Audit.Handlers;

using System.Text.Json;

using Application.Services;

using Domain;

using RabbitMQ.Module.Contracts;

using Shared.Messaging.Events;

public class TaskCreatedHandler(IAuditService auditService, ILogger<TaskCreatedHandler> logger)
    : BaseAuditHandler(auditService, logger), IMessageHandler<TaskCreatedEvent>
{

    #region Methods

    public async Task HandleAsync(TaskCreatedEvent message, IMessageContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Audit: Task created {TaskId}", message.TaskId);

        await LogAuditAsync(
            message.CreatedBy,
            null,
            AuditAction.Create,
            EntityType.Task,
            message.TaskId.ToString(),
            null,
            JsonSerializer.Serialize(message),
            context,
            cancellationToken);
    }

    #endregion

}

public class TaskAssignedHandler(IAuditService auditService, ILogger<TaskAssignedHandler> logger)
    : BaseAuditHandler(auditService, logger), IMessageHandler<TaskAssignedEvent>
{

    #region Methods

    public async Task HandleAsync(TaskAssignedEvent message, IMessageContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Audit: Task assigned {TaskId} to {AssigneeId}", message.TaskId, message.AssigneeId);

        await LogAuditAsync(
            message.AssignedBy,
            null,
            AuditAction.Assign,
            EntityType.Task,
            message.TaskId.ToString(),
            null,
            JsonSerializer.Serialize(message),
            context,
            cancellationToken);
    }

    #endregion

}

public class TaskStatusChangedHandler(IAuditService auditService, ILogger<TaskStatusChangedHandler> logger)
    : BaseAuditHandler(auditService, logger), IMessageHandler<TaskStatusChangedEvent>
{

    #region Methods

    public async Task HandleAsync(TaskStatusChangedEvent message, IMessageContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Audit: Task status changed {TaskId} from {OldStatus} to {NewStatus}",
            message.TaskId,
            message.OldStatus,
            message.NewStatus);

        await LogAuditAsync(
            message.ChangedBy,
            null,
            AuditAction.StatusChange,
            EntityType.Task,
            message.TaskId.ToString(),
            message.OldStatus,
            message.NewStatus,
            context,
            cancellationToken);
    }

    #endregion

}

public class TaskDeletedHandler(IAuditService auditService, ILogger<TaskAssignedHandler> logger)
    : BaseAuditHandler(auditService, logger), IMessageHandler<TaskDeletedEvent>
{

    #region Methods

    public async Task HandleAsync(TaskDeletedEvent message, IMessageContext context, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing TaskDeleted event for Task {TaskId} by User {DeletedBy}",
            message.TaskId,
            message.DeletedBy);

        await LogAuditAsync(
            message.DeletedBy,
            null,
            AuditAction.Create,
            EntityType.Task,
            message.TaskId.ToString(),
            null,
            JsonSerializer.Serialize(message),
            context,
            cancellationToken);
    }

    #endregion

}
