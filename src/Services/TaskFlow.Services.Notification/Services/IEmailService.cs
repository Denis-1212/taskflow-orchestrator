namespace TaskFlow.Services.Notification.Services;

using Models;

public interface IEmailService
{

    #region Methods

    Task SendEmailAsync(EmailMessage emailMessage);

    #endregion

}
