namespace TaskFlow.Services.Task.Domain;

public class OutboxMessage
{

    #region Properties

    public Guid Id { get; private set; }
    public string EventType { get; private set; }
    public string EventData { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public int RetryCount { get; private set; }
    public string? ErrorMessage { get; private set; }

    #endregion

    #region Constructors

    public OutboxMessage(string eventType, string eventData)
    {
        Id = Guid.NewGuid();
        EventType = eventType;
        EventData = eventData;
        CreatedAt = DateTime.UtcNow;
        RetryCount = 0;
    }

    private OutboxMessage()
    {
        EventType = string.Empty;
        EventData = string.Empty;
    }

    #endregion

    #region Methods

    public void MarkProcessed()
    {
        ProcessedAt = DateTime.UtcNow;
    }

    public void IncrementRetry(string error)
    {
        RetryCount++;
        ErrorMessage = error;
    }

    public bool CanRetry(int maxRetries)
    {
        return RetryCount < maxRetries;
    }

    #endregion

}
