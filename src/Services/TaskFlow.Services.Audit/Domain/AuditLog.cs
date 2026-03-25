namespace TaskFlow.Services.Audit.Domain;

public class AuditLog
{

    #region Properties

    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; }
    public string UserEmail { get; private set; }
    public string Action { get; private set; }
    public string EntityType { get; private set; }
    public string EntityId { get; private set; }
    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }
    public string IpAddress { get; private set; }
    public string UserAgent { get; private set; }
    public DateTime Timestamp { get; private set; }

    #endregion

    #region Constructors

    public AuditLog(
        Guid? userId,
        string userEmail,
        string action,
        string entityType,
        string entityId,
        string? oldValue,
        string? newValue,
        string ipAddress,
        string userAgent)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        UserEmail = userEmail;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        OldValue = oldValue;
        NewValue = newValue;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        Timestamp = DateTime.UtcNow;
    }

    private AuditLog()
    {
        UserEmail = string.Empty;
        Action = string.Empty;
        EntityType = string.Empty;
        EntityId = string.Empty;
        IpAddress = string.Empty;
        UserAgent = string.Empty;
    }

    #endregion

}
