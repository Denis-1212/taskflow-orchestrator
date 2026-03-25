namespace TaskFlow.Services.Audit.Handlers;

using System.Text.Json;

using Application.Services;

using RabbitMQ.Module.Contracts;

using Shared.Messaging.Events;

public class TaskCreatedHandler : BaseAuditHandler, IMessageHandler<TaskCreatedEvent>
{

    #region Constructors

    public TaskCreatedHandler(IAuditService auditService, ILogger<TaskCreatedHandler> logger)
        : base(auditService, logger)
    {
    }

    #endregion

    #region Methods

    public async Task HandleAsync(TaskCreatedEvent message, IMessageContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Audit: Task created {TaskId}", message.TaskId);

        await LogAuditAsync(
            message.CreatedBy,
            null,
            "CREATE",
            "Task",
            message.TaskId.ToString(),
            null,
            JsonSerializer.Serialize(message),
            context,
            cancellationToken);
    }

    #endregion

}

public class TaskAssignedHandler : BaseAuditHandler, IMessageHandler<TaskAssignedEvent>
{

    #region Constructors

    public TaskAssignedHandler(IAuditService auditService, ILogger<TaskAssignedHandler> logger)
        : base(auditService, logger)
    {
    }

    #endregion

    #region Methods

    public async Task HandleAsync(TaskAssignedEvent message, IMessageContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Audit: Task assigned {TaskId} to {AssigneeId}", message.TaskId, message.AssigneeId);

        await LogAuditAsync(
            message.AssignedBy,
            null,
            "ASSIGN",
            "Task",
            message.TaskId.ToString(),
            null,
            JsonSerializer.Serialize(message),
            context,
            cancellationToken);
    }

    #endregion

}

public class TaskStatusChangedHandler : BaseAuditHandler, IMessageHandler<TaskStatusChangedEvent>
{

    #region Constructors

    public TaskStatusChangedHandler(IAuditService auditService, ILogger<TaskStatusChangedHandler> logger)
        : base(auditService, logger)
    {
    }

    #endregion

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
            "STATUS_CHANGE",
            "Task",
            message.TaskId.ToString(),
            message.OldStatus,
            message.NewStatus,
            context,
            cancellationToken);
    }

    #endregion

}
