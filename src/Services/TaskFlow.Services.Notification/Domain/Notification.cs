namespace TaskFlow.Services.Notification.Domain;

public class Notification
{

    #region Properties

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Title { get; private set; }
    public string Content { get; private set; }
    public string Metadata { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ReadAt { get; private set; }

    #endregion

    #region Constructors

    public Notification(Guid userId, NotificationType type, string title, string content, string metadata)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Type = type;
        Title = title;
        Content = content;
        Metadata = metadata ?? string.Empty;
        IsRead = false;
        CreatedAt = DateTime.UtcNow;
    }

    private Notification()
    {
        Title = string.Empty;
        Content = string.Empty;
        Metadata = string.Empty;
    }

    #endregion

    #region Methods

    public void MarkAsRead()
    {
        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }

    #endregion

}
