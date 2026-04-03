namespace TaskFlow.Services.Notification.Handlers;

using Application.Services;

using Domain;

using RabbitMQ.Module.Contracts;

using Shared.Messaging.Events;

using Task = System.Threading.Tasks.Task;

public class TaskStatusChangedHandler(INotificationService notificationService, ILogger<TaskStatusChangedHandler> logger)
    : IMessageHandler<TaskStatusChangedEvent>
{

    #region Methods

    public async Task HandleAsync(TaskStatusChangedEvent message, IMessageContext context, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing TaskStatusChanged event for Task {TaskId} from {OldStatus} to {NewStatus}",
            message.TaskId,
            message.OldStatus,
            message.NewStatus);

        // Уведомление автору изменения
        await notificationService.CreateInAppNotificationAsync(
            message.ChangedBy,
            NotificationType.TaskStatusChanged,
            $"Task Status Changed: {message.TaskTitle}",
            $"Task '{message.TaskTitle}' status changed from {message.OldStatus} to {message.NewStatus}",
            $"{{\"taskId\":\"{message.TaskId}\",\"projectId\":\"{message.ProjectId}\"}}");
    }

    #endregion

}
