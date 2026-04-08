namespace TaskFlow.Services.Notification.Handlers;

using Application.Services;

using Domain;

using RabbitMQ.Module.Contracts;

using Shared.Messaging.Events;

public class TaskDeletedHandler(INotificationService notificationService, ILogger<TaskDeletedHandler> logger)
    : IMessageHandler<TaskDeletedEvent>
{

    #region Methods

    public async Task HandleAsync(TaskDeletedEvent message, IMessageContext context, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing TaskDeleted event for Task {TaskId} by User {DeletedBy}",
            message.TaskId,
            message.DeletedBy);

        await notificationService.CreateInAppNotificationAsync(
            message.DeletedBy,
            NotificationType.TaskDeleted,
            $"Task Deleted: {message.TaskTitle}",
            $"You have been deleted task '{message.TaskTitle}' in project '{message.ProjectId}' due on {message.OccurredAt:dd.MM.yyyy}",
            $"{{\"taskId\":\"{message.TaskId}\",\"projectId\":\"{message.ProjectId}\"}}");
    }

    #endregion

}
