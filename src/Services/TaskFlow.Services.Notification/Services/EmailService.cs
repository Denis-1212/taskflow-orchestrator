namespace TaskFlow.Services.Notification.Services;

using MailKit.Net.Smtp;
using MailKit.Security;

using Microsoft.Extensions.Options;

using MimeKit;
using MimeKit.Text;

using Models;

using Settings;

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
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
            message.To.Add(new MailboxAddress(emailMessage.ToName, emailMessage.ToEmail));
            message.Subject = emailMessage.Subject;
            message.Body = new TextPart(TextFormat.Html)
            {
                Text = emailMessage.Body
            };

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
        }
    }

    #endregion

}
