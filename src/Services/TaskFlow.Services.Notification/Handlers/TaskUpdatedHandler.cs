namespace TaskFlow.Services.Notification.Handlers;

using Application.Services;

using Domain;

using RabbitMQ.Module.Contracts;

using Shared.Messaging.Events;

public class TaskUpdatedHandler(INotificationService notificationService, ILogger<TaskUpdatedHandler> logger)
    : IMessageHandler<TaskUpdatedEvent>
{

    #region Methods

    public async Task HandleAsync(TaskUpdatedEvent message, IMessageContext context, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing TaskUpdated event for Task {TaskId} by User {UpdatedBy}",
            message.TaskId,
            message.UpdatedBy);

        await notificationService.CreateInAppNotificationAsync(
            message.UpdatedBy,
            NotificationType.TaskUpdated,
            $"Task Updated: {message.TaskTitle}",
            $"You have been Updated task '{message.TaskTitle}' in project '{message.ProjectId}' due on {message.OccurredAt:dd.MM.yyyy}",
            $"{{\"taskId\":\"{message.TaskId}\",\"projectId\":\"{message.ProjectId}\"}}");
    }

    #endregion

}
