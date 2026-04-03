namespace TaskFlow.Services.Notification.Services;

using Models;

using Task = System.Threading.Tasks.Task;

public interface IEmailService
{

    #region Methods

    Task SendEmailAsync(EmailMessage emailMessage);

    #endregion

}
