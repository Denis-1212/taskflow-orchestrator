namespace TaskFlow.Services.Notification.Handlers;

using Application.Services;

using Auth;

using Clients;

using Domain;

using Models;

using RabbitMQ.Module.Contracts;

using Services;

using Shared.Messaging.Events;

public class TaskCreatedHandler(
    INotificationService notificationService,
    IEmailService emailService,
    IAuthGrpcClient authGrpcClient,
    ILogger<TaskCreatedHandler> logger)
    : IMessageHandler<TaskCreatedEvent>
{

    #region Methods

    public async Task HandleAsync(TaskCreatedEvent message, IMessageContext context, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing TaskCreated event for Task {TaskId}", message.TaskId);

        await notificationService.CreateInAppNotificationAsync(
            message.CreatedBy,
            NotificationType.TaskCreated,
            $"Task Created: {message.TaskTitle}",
            $"Task '{message.TaskTitle}' has been created in project {message.ProjectId}",
            $"{{\"taskId\":\"{message.TaskId}\",\"projectId\":\"{message.ProjectId}\"}}");

        string emailBody = $@"
            <h1>Task Created</h1>
            <p>Your task '<strong>{message.TaskTitle}</strong>' has been successfully created.</p>
            <p>Project ID: {message.ProjectId}</p>
            <p>Due Date: {message.DueDate:dd.MM.yyyy}</p>
        ";

        GetUserResponse userCreator = await authGrpcClient.GetUserAsync(message.CreatedBy);

        var emailMessage = new EmailMessage
        {
            ToEmail = userCreator.Email,
            ToName = userCreator.FullName,
            Subject = $"Task Created: {message.TaskTitle}",
            Body = emailBody
        };

        await emailService.SendEmailAsync(emailMessage);

        if (message.AssigneeId.HasValue)
        {
            await notificationService.CreateInAppNotificationAsync(
                message.AssigneeId.Value,
                NotificationType.TaskAssigned,
                $"Task Assigned: {message.TaskTitle}",
                $"You have been assigned to task '{message.TaskTitle}' due on {message.DueDate:dd.MM.yyyy}",
                $"{{\"taskId\":\"{message.TaskId}\",\"projectId\":\"{message.ProjectId}\"}}");

            GetUserResponse assignedUser = await authGrpcClient.GetUserAsync(message.CreatedBy);
            emailMessage = new EmailMessage
            {
                ToEmail = assignedUser.Email,
                ToName = assignedUser.FullName,
                Subject = $"Task Created: {message.TaskTitle}",
                Body = emailBody
            };

            await emailService.SendEmailAsync(emailMessage);
        }
    }

    #endregion

}
