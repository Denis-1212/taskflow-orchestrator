namespace TaskFlow.Services.Notification.Handlers;

using Application.Services;

using Domain;

using RabbitMQ.Module.Contracts;

using Shared.Messaging.Events;

using Task = Task;

public class TaskCreatedHandler(INotificationService notificationService, ILogger<TaskCreatedHandler> logger)
    : IMessageHandler<TaskCreatedEvent>
{

    #region Methods

    public async Task HandleAsync(TaskCreatedEvent message, IMessageContext context, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing TaskCreated event for Task {TaskId}", message.TaskId);

        // Уведомление создателю задачи
        await notificationService.CreateInAppNotificationAsync(
            message.CreatedBy,
            NotificationType.TaskCreated,
            $"Task Created: {message.TaskTitle}",
            $"Task '{message.TaskTitle}' has been created in project {message.ProjectId}",
            $"{{\"taskId\":\"{message.TaskId}\",\"projectId\":\"{message.ProjectId}\"}}");

        // Если есть исполнитель — уведомить его
        if (message.AssigneeId.HasValue)
        {
            await notificationService.CreateInAppNotificationAsync(
                message.AssigneeId.Value,
                NotificationType.TaskAssigned,
                $"Task Assigned: {message.TaskTitle}",
                $"You have been assigned to task '{message.TaskTitle}' due on {message.DueDate:dd.MM.yyyy}",
                $"{{\"taskId\":\"{message.TaskId}\",\"projectId\":\"{message.ProjectId}\"}}");
        }
    }

    #endregion

}
