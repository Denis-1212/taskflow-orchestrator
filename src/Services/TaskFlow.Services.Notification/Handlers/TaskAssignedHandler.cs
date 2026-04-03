namespace TaskFlow.Services.Notification.Handlers;

using Application.Services;

using Domain;

using RabbitMQ.Module.Contracts;

using Shared.Messaging.Events;

using Task = System.Threading.Tasks.Task;

public class TaskAssignedHandler(INotificationService notificationService, ILogger<TaskAssignedHandler> logger)
    : IMessageHandler<TaskAssignedEvent>
{

    #region Methods

    public async Task HandleAsync(TaskAssignedEvent message, IMessageContext context, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing TaskAssigned event for Task {TaskId} to User {AssigneeId}",
            message.TaskId,
            message.AssigneeId);

        await notificationService.CreateInAppNotificationAsync(
            message.AssigneeId,
            NotificationType.TaskAssigned,
            $"Task Assigned: {message.TaskTitle}",
            $"You have been assigned to task '{message.TaskTitle}' in project '{message.ProjectName}' due on {message.DueDate:dd.MM.yyyy}",
            $"{{\"taskId\":\"{message.TaskId}\",\"projectId\":\"{message.ProjectId}\"}}");
    }

    #endregion

}
