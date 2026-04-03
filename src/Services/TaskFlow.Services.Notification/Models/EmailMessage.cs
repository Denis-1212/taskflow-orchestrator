namespace TaskFlow.Services.Notification.Models;

public class EmailMessage
{

    #region Properties

    public string ToEmail { get; set; } = string.Empty;
    public string ToName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    #endregion

}
