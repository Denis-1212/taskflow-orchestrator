namespace TaskFlow.Services.Notification.Services;

using MailKit.Net.Smtp;
using MailKit.Security;

using Microsoft.Extensions.Options;

using MimeKit;
using MimeKit.Text;

using Models;

using Settings;

using Task = System.Threading.Tasks.Task;

public class EmailService(IOptions<SmtpSettings> smtpSettings, ILogger<EmailService> logger) : IEmailService
{

    #region Fields

    private readonly SmtpSettings _smtpSettings = smtpSettings.Value;

    #endregion

    #region Methods

    public async Task SendEmailAsync(EmailMessage emailMessage)
    {
        try
        {
            // Создаем MimeMessage (само письмо)
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
            message.To.Add(new MailboxAddress(emailMessage.ToName, emailMessage.ToEmail));
            message.Subject = emailMessage.Subject;
            message.Body = new TextPart(TextFormat.Html)
            {
                Text = emailMessage.Body
            };

            // Отправляем письмо через SMTP клиент
            using var client = new SmtpClient();
            await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            logger.LogInformation("Email sent successfully to {Email}", emailMessage.ToEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {Email}", emailMessage.ToEmail);
            // Здесь можно добавить retry-логику или отправить письмо в Dead Letter, но для начала просто логируем ошибку
        }
    }

    #endregion

}
