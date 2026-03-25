namespace TaskFlow.Services.Audit.Tests;

using System.Reflection;

using Application.Services;

using Domain;

using FluentAssertions;

using Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

using Shared.Kernel;

public class AuditServiceTests : IDisposable
{

    #region Fields

    private readonly AuditDbContext _context;
    private readonly Mock<ILogger<AuditService>> _loggerMock;
    private readonly AuditService _auditService;

    #endregion

    #region Constructors

    public AuditServiceTests()
    {
        _context = TestDatabase.Create();
        _loggerMock = new Mock<ILogger<AuditService>>();
        _auditService = new AuditService(_context, _loggerMock.Object);
    }

    #endregion

    #region Methods

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task LogAsync_ShouldCreateAuditLog()
    {
        // Arrange
        var userId = Guid.NewGuid();
        string userEmail = "user@example.com";
        string action = "CREATE";
        string entityType = "Task";
        string entityId = Guid.NewGuid().ToString();
        string? oldValue = null;
        string newValue = "{\"title\":\"Test\"}";
        string ipAddress = "127.0.0.1";
        string userAgent = "Mozilla/5.0";

        // Act
        Result result = await _auditService.LogAsync(
                            userId,
                            userEmail,
                            action,
                            entityType,
                            entityId,
                            oldValue,
                            newValue,
                            ipAddress,
                            userAgent);

        // Assert
        result.IsSuccess.Should().BeTrue();

        AuditLog? log = await _context.AuditLogs.FirstOrDefaultAsync();
        log.Should().NotBeNull();
        log!.UserId.Should().Be(userId);
        log.UserEmail.Should().Be(userEmail);
        log.Action.Should().Be(action);
        log.EntityType.Should().Be(entityType);
        log.EntityId.Should().Be(entityId);
        log.NewValue.Should().Be(newValue);
        log.IpAddress.Should().Be(ipAddress);
        log.UserAgent.Should().Be(userAgent);
    }

    [Fact]
    public async Task LogAsync_WithoutUserId_ShouldCreateAuditLog()
    {
        // Arrange
        string userEmail = "system@example.com";
        string action = "SYSTEM";
        string entityType = "Config";
        string entityId = "1";
        string ipAddress = "127.0.0.1";
        string userAgent = "system";

        // Act
        Result result = await _auditService.LogAsync(
                            null,
                            userEmail,
                            action,
                            entityType,
                            entityId,
                            null,
                            null,
                            ipAddress,
                            userAgent);

        // Assert
        result.IsSuccess.Should().BeTrue();

        AuditLog? log = await _context.AuditLogs.FirstOrDefaultAsync();
        log.Should().NotBeNull();
        log!.UserId.Should().BeNull();
        log.UserEmail.Should().Be(userEmail);
    }

    [Fact]
    public async Task SearchAsync_WithNoFilters_ShouldReturnAllLogs()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
        {
            var log = new AuditLog(
                Guid.NewGuid(),
                $"user{i}@example.com",
                "CREATE",
                "Task",
                i.ToString(),
                null,
                null,
                "127.0.0.1",
                "agent");

            _context.AuditLogs.Add(log);
        }

        await _context.SaveChangesAsync();

        // Act
        Result<IEnumerable<AuditLogResult>> result = await _auditService.SearchAsync(null, null, null, null, null, null, 1, 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(5);
    }

    [Fact]
    public async Task SearchAsync_WithUserIdFilter_ShouldReturnOnlyUserLogs()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var userLog = new AuditLog(userId, "user@example.com", "CREATE", "Task", "1", null, null, "127.0.0.1", "agent");
        var otherLog = new AuditLog(otherUserId, "other@example.com", "CREATE", "Task", "2", null, null, "127.0.0.1", "agent");

        _context.AuditLogs.AddRange(userLog, otherLog);
        await _context.SaveChangesAsync();

        // Act
        Result<IEnumerable<AuditLogResult>> result = await _auditService.SearchAsync(userId, null, null, null, null, null, 1, 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().UserId.Should().Be(userId);
    }

    [Fact]
    public async Task SearchAsync_WithActionFilter_ShouldReturnOnlyMatchingActions()
    {
        // Arrange
        var createLog = new AuditLog(null, "user@example.com", "CREATE", "Task", "1", null, null, "127.0.0.1", "agent");
        var updateLog = new AuditLog(null, "user@example.com", "UPDATE", "Task", "1", null, null, "127.0.0.1", "agent");

        _context.AuditLogs.AddRange(createLog, updateLog);
        await _context.SaveChangesAsync();

        // Act
        Result<IEnumerable<AuditLogResult>> result = await _auditService.SearchAsync(null, "CREATE", null, null, null, null, 1, 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Action.Should().Be("CREATE");
    }

    [Fact]
    public async Task SearchAsync_WithEntityTypeFilter_ShouldReturnOnlyMatchingEntities()
    {
        // Arrange
        var taskLog = new AuditLog(null, "user@example.com", "CREATE", "Task", "1", null, null, "127.0.0.1", "agent");
        var projectLog = new AuditLog(null, "user@example.com", "CREATE", "Project", "1", null, null, "127.0.0.1", "agent");

        _context.AuditLogs.AddRange(taskLog, projectLog);
        await _context.SaveChangesAsync();

        // Act
        Result<IEnumerable<AuditLogResult>> result = await _auditService.SearchAsync(null, null, "Task", null, null, null, 1, 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().EntityType.Should().Be("Task");
    }

    [Fact]
    public async Task SearchAsync_WithDateRangeFilter_ShouldReturnLogsInRange()
    {
        // Arrange
        var oldLog = new AuditLog(null, "user@example.com", "CREATE", "Task", "1", null, null, "127.0.0.1", "agent");
        var recentLog = new AuditLog(null, "user@example.com", "CREATE", "Task", "2", null, null, "127.0.0.1", "agent");

        // Set timestamps using reflection since Timestamp is read-only
        PropertyInfo? oldLogField = typeof(AuditLog).GetProperty("Timestamp");
        oldLogField?.SetValue(oldLog, DateTime.UtcNow.AddDays(-10));

        _context.AuditLogs.AddRange(oldLog, recentLog);
        await _context.SaveChangesAsync();

        DateTime fromDate = DateTime.UtcNow.AddDays(-5);
        DateTime toDate = DateTime.UtcNow.AddDays(1);

        // Act
        Result<IEnumerable<AuditLogResult>> result = await _auditService.SearchAsync(null, null, null, null, fromDate, toDate, 1, 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        for (int i = 1; i <= 15; i++)
        {
            var log = new AuditLog(null, $"user{i}@example.com", "CREATE", "Task", i.ToString(), null, null, "127.0.0.1", "agent");
            _context.AuditLogs.Add(log);
        }

        await _context.SaveChangesAsync();

        // Act - page 2 with 5 items per page
        Result<IEnumerable<AuditLogResult>> result = await _auditService.SearchAsync(null, null, null, null, null, null, 2, 5);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(5);
    }

    [Fact]
    public async Task CleanupOldLogsAsync_ShouldRemoveLogsOlderThanRetentionDays()
    {
        // Arrange
        var oldLog = new AuditLog(null, "user@example.com", "CREATE", "Task", "1", null, null, "127.0.0.1", "agent");
        var newLog = new AuditLog(null, "user@example.com", "CREATE", "Task", "2", null, null, "127.0.0.1", "agent");

        PropertyInfo? oldLogField = typeof(AuditLog).GetProperty("Timestamp");
        oldLogField?.SetValue(oldLog, DateTime.UtcNow.AddDays(-100));
        oldLogField?.SetValue(newLog, DateTime.UtcNow.AddDays(-1));

        _context.AuditLogs.AddRange(oldLog, newLog);
        await _context.SaveChangesAsync();

        // Act
        Result result = await _auditService.CleanupOldLogsAsync(90);

        // Assert
        result.IsSuccess.Should().BeTrue();

        List<AuditLog> remainingLogs = await _context.AuditLogs.ToListAsync();
        remainingLogs.Should().HaveCount(1);
        remainingLogs[0].EntityId.Should().Be("2");
    }

    [Fact]
    public async Task CleanupOldLogsAsync_WithNoOldLogs_ShouldNotRemoveAnything()
    {
        // Arrange
        var recentLog = new AuditLog(null, "user@example.com", "CREATE", "Task", "1", null, null, "127.0.0.1", "agent");
        _context.AuditLogs.Add(recentLog);
        await _context.SaveChangesAsync();

        // Act
        Result result = await _auditService.CleanupOldLogsAsync(90);

        // Assert
        result.IsSuccess.Should().BeTrue();

        List<AuditLog> logs = await _context.AuditLogs.ToListAsync();
        logs.Should().HaveCount(1);
    }

    #endregion

}
