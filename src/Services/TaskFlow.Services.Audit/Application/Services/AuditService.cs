namespace TaskFlow.Services.Audit.Application.Services;

using Domain;

using Infrastructure;

using Microsoft.EntityFrameworkCore;

using Shared.Kernel;

public class AuditService : IAuditService
{

    #region Fields

    private readonly AuditDbContext _context;
    private readonly ILogger<AuditService> _logger;

    #endregion

    #region Constructors

    public AuditService(AuditDbContext context, ILogger<AuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #endregion

    #region Methods

    public async Task<Result> LogAsync(
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
        _logger.LogInformation(
            "Logging audit: {Action} on {EntityType} {EntityId} by {UserEmail}",
            action,
            entityType,
            entityId,
            userEmail);

        var auditLog = new AuditLog(
            userId,
            userEmail,
            action,
            entityType,
            entityId,
            oldValue,
            newValue,
            ipAddress,
            userAgent);

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<IEnumerable<AuditLogResult>>> SearchAsync(
        Guid? userId,
        string? action,
        string? entityType,
        string? entityId,
        DateTime? from,
        DateTime? to,
        int page = 1,
        int pageSize = 20)
    {
        IQueryable<AuditLog> query = _context.AuditLogs.AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(l => l.UserId == userId.Value);
        }

        if (!string.IsNullOrEmpty(action))
        {
            query = query.Where(l => l.Action == action);
        }

        if (!string.IsNullOrEmpty(entityType))
        {
            query = query.Where(l => l.EntityType == entityType);
        }

        if (!string.IsNullOrEmpty(entityId))
        {
            query = query.Where(l => l.EntityId == entityId);
        }

        if (from.HasValue)
        {
            query = query.Where(l => l.Timestamp >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(l => l.Timestamp <= to.Value);
        }

        List<AuditLog> logs = await query
                                  .OrderByDescending(l => l.Timestamp)
                                  .Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToListAsync();

        return logs.Select(MapToResult).ToList();
    }

    public async Task<Result> CleanupOldLogsAsync(int retentionDays)
    {
        _logger.LogInformation("Cleaning up audit logs older than {RetentionDays} days", retentionDays);

        DateTime cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

        List<AuditLog> oldLogs = await _context.AuditLogs
                                     .Where(l => l.Timestamp < cutoffDate)
                                     .ToListAsync();

        if (oldLogs.Any())
        {
            _context.AuditLogs.RemoveRange(oldLogs);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Removed {Count} old audit logs", oldLogs.Count);
        }

        return Result.Success();
    }

    private static AuditLogResult MapToResult(AuditLog log)
    {
        return new AuditLogResult(
            log.Id,
            log.UserId,
            log.UserEmail,
            log.Action,
            log.EntityType,
            log.EntityId,
            log.OldValue,
            log.NewValue,
            log.IpAddress,
            log.UserAgent,
            log.Timestamp);
    }

    #endregion

}
