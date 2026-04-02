namespace TaskFlow.Services.Notification.Domain;

public enum NotificationType
{
    TaskCreated,
    TaskAssigned,
    TaskStatusChanged,
    TaskDeleted,
    ProjectInvite,
    DueDateReminder
}
