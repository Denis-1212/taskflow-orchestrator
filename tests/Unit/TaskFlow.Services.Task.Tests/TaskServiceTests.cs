namespace TaskFlow.Services.Task.Tests;

using Application.Services;

using Clients;

using Domain;

using FluentAssertions;

using Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

using RabbitMQ.Module.Contracts;

using Shared.Kernel;

using Task = System.Threading.Tasks.Task;

public class TaskServiceTests : IDisposable
{

    #region Fields

    private readonly TaskDbContext _context;
    private readonly Mock<IProjectGrpcClient> _projectClientMock;
    private readonly TaskService _taskService;

    #endregion

    #region Constructors

    public TaskServiceTests()
    {
        _context = TestDatabase.Create();
        _projectClientMock = new Mock<IProjectGrpcClient>();
        var authGrpcClientMock = new Mock<IAuthGrpcClient>();
        var publisherMock = new Mock<IPublisher>();
        var loggerMock = new Mock<ILogger<TaskService>>();
        _taskService = new TaskService(_context, _projectClientMock.Object, authGrpcClientMock.Object, publisherMock.Object, loggerMock.Object);
    }

    #endregion

    #region Methods

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateTask()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        DateTime dueDate = DateTime.UtcNow.AddDays(7);

        _projectClientMock.Setup(x => x.ProjectExistsAsync(projectId))
            .ReturnsAsync(true);

        _projectClientMock.Setup(x => x.ValidateMemberAsync(projectId, It.IsAny<Guid>()))
            .ReturnsAsync((true, "Member"));

        // Act
        Result<TaskResult> result = await _taskService.CreateAsync(
                                        projectId,
                                        "Test Task",
                                        "Description",
                                        "Medium",
                                        null,
                                        createdBy,
                                        dueDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Title.Should().Be("Test Task");
        result.Value.ProjectId.Should().Be(projectId);

        TaskItem? taskInDb = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == result.Value.Id);
        taskInDb.Should().NotBeNull();
        taskInDb!.Title.Should().Be("Test Task");

        // Check outbox message
        OutboxMessage? outboxMessage = await _context.OutboxMessages.FirstOrDefaultAsync();
        outboxMessage.Should().NotBeNull();
        outboxMessage!.EventType.Should().Be("TaskCreatedEvent");
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentProject_ShouldReturnNotFound()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();

        _projectClientMock.Setup(x => x.ProjectExistsAsync(projectId))
            .ReturnsAsync(false);

        // Act
        Result<TaskResult> result = await _taskService.CreateAsync(
                                        projectId,
                                        "Test Task",
                                        "Description",
                                        "Medium",
                                        null,
                                        createdBy,
                                        DateTime.UtcNow.AddDays(7));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidPriority_ShouldReturnValidationError()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();

        _projectClientMock.Setup(x => x.ProjectExistsAsync(projectId))
            .ReturnsAsync(true);

        // Act
        Result<TaskResult> result = await _taskService.CreateAsync(
                                        projectId,
                                        "Test Task",
                                        "Description",
                                        "InvalidPriority",
                                        null,
                                        createdBy,
                                        DateTime.UtcNow.AddDays(7));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Validation);
        result.Error.Message.Should().Contain("Invalid priority");
    }

    [Fact]
    public async Task CreateAsync_WithNonMemberAssignee_ShouldReturnValidationError()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var assigneeId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();

        _projectClientMock.Setup(x => x.ProjectExistsAsync(projectId))
            .ReturnsAsync(true);

        _projectClientMock.Setup(x => x.ValidateMemberAsync(projectId, assigneeId))
            .ReturnsAsync((false, string.Empty));

        // Act
        Result<TaskResult> result = await _taskService.CreateAsync(
                                        projectId,
                                        "Test Task",
                                        "Description",
                                        "Medium",
                                        assigneeId,
                                        createdBy,
                                        DateTime.UtcNow.AddDays(7));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Validation);
        result.Error.Message.Should().Contain("not a member");
    }

    [Fact]
    public async Task GetByIdAsync_WithValidTask_ShouldReturnTask()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var taskItem = new TaskItem(
            projectId,
            "Test Task",
            "Description",
            TaskPriority.Medium,
            null,
            userId,
            DateTime.UtcNow.AddDays(7));

        _context.Tasks.Add(taskItem);
        await _context.SaveChangesAsync();

        _projectClientMock.Setup(x => x.ValidateMemberAsync(projectId, userId))
            .ReturnsAsync((true, "Member"));

        // Act
        Result<TaskResult> result = await _taskService.GetByIdAsync(taskItem.Id, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(taskItem.Id);
        result.Value.Title.Should().Be("Test Task");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonMember_ShouldReturnForbidden()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var nonMemberId = Guid.NewGuid();
        var task = new TaskItem(
            projectId,
            "Test Task",
            "Description",
            TaskPriority.Medium,
            null,
            ownerId,
            DateTime.UtcNow.AddDays(7));

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        _projectClientMock.Setup(x => x.ValidateMemberAsync(projectId, nonMemberId))
            .ReturnsAsync((false, string.Empty));

        // Act
        Result<TaskResult> result = await _taskService.GetByIdAsync(task.Id, nonMemberId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task DeleteAsync_WithValidTask_ShouldSoftDelete()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var task = new TaskItem(
            projectId,
            "Test Task",
            "Description",
            TaskPriority.Medium,
            null,
            userId,
            DateTime.UtcNow.AddDays(7));

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        _projectClientMock.Setup(x => x.ValidateMemberAsync(projectId, userId))
            .ReturnsAsync((true, "Member"));

        // Act
        Result result = await _taskService.DeleteAsync(task.Id, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();

        TaskItem? deletedTask = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == task.Id);
        deletedTask!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetTasksAsync_WithProjectId_ShouldReturnTasks()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var task1 = new TaskItem(projectId, "Task 1", "Desc", TaskPriority.Medium, null, userId, DateTime.UtcNow.AddDays(7));
        var task2 = new TaskItem(projectId, "Task 2", "Desc", TaskPriority.High, null, userId, DateTime.UtcNow.AddDays(7));

        _context.Tasks.AddRange(task1, task2);
        await _context.SaveChangesAsync();

        _projectClientMock.Setup(x => x.ValidateMemberAsync(projectId, userId))
            .ReturnsAsync((true, "Member"));

        // Act
        Result<IEnumerable<TaskResult>> result = await _taskService.GetTasksAsync(projectId, null, null, null, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnCorrectStatistics()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var task1 = new TaskItem(projectId, "Task 1", "Desc", TaskPriority.Medium, null, userId, DateTime.UtcNow.AddDays(7));
        task1.ChangeStatus(TaskItemStatus.InProgress, userId);

        var task2 = new TaskItem(projectId, "Task 2", "Desc", TaskPriority.High, null, userId, DateTime.UtcNow.AddDays(7));
        task2.ChangeStatus(TaskItemStatus.Completed, userId);

        var task3 = new TaskItem(projectId, "Task 3", "Desc", TaskPriority.Low, null, userId, DateTime.UtcNow.AddDays(7));

        _context.Tasks.AddRange(task1, task2, task3);
        await _context.SaveChangesAsync();

        _projectClientMock.Setup(x => x.ValidateMemberAsync(projectId, userId))
            .ReturnsAsync((true, "Member"));

        // Act
        Result<TaskStatisticsResult> result = await _taskService.GetStatisticsAsync(projectId, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Total.Should().Be(3);
        result.Value.Todo.Should().Be(1);
        result.Value.InProgress.Should().Be(1);
        result.Value.Completed.Should().Be(1);
        result.Value.Cancelled.Should().Be(0);
    }

    #endregion

}
