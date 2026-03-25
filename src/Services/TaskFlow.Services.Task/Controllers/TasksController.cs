namespace TaskFlow.Services.Task.Controllers;

using System.Security.Claims;

using Application.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Shared.DTOs;
using Shared.Kernel;

[ApiController]
[Authorize]
[Route("api/tasks")]
public class TasksController : ControllerBase
{

    #region Fields

    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    #endregion

    #region Constructors

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    #endregion

    #region Methods

    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create(CreateTaskDto request)
    {
        Guid userId = GetCurrentUserId();

        _logger.LogInformation("Creating task in project {ProjectId}", request.ProjectId);

        Result<TaskResult> result = await _taskService.CreateAsync(
                                        request.ProjectId,
                                        request.Title,
                                        request.Description,
                                        request.Priority,
                                        request.AssigneeId,
                                        userId,
                                        request.DueDate);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error),
                ErrorType.Validation => BadRequest(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return Ok(MapToDto(result.Value));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskDto>> GetById(Guid id)
    {
        Guid userId = GetCurrentUserId();

        Result<TaskResult> result = await _taskService.GetByIdAsync(id, userId);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error),
                ErrorType.Forbidden => Forbidden(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return Ok(MapToDto(result.Value));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasks(
        [FromQuery] Guid? projectId,
        [FromQuery] string? status,
        [FromQuery] string? priority,
        [FromQuery] Guid? assigneeId)
    {
        Guid userId = GetCurrentUserId();

        Result<IEnumerable<TaskResult>> result = await _taskService.GetTasksAsync(projectId, status, priority, assigneeId, userId);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.Forbidden => Forbidden(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return Ok(result.Value.Select(MapToDto));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TaskDto>> Update(Guid id, UpdateTaskDto request)
    {
        Guid userId = GetCurrentUserId();

        Result<TaskResult> result = await _taskService.UpdateAsync(
                                        id,
                                        request.Title,
                                        request.Description,
                                        request.Priority,
                                        request.DueDate,
                                        userId);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error),
                ErrorType.Forbidden => Forbidden(result.Error),
                ErrorType.Validation => BadRequest(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return Ok(MapToDto(result.Value));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        Guid userId = GetCurrentUserId();

        Result result = await _taskService.DeleteAsync(id, userId);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error),
                ErrorType.Forbidden => Forbidden(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<TaskDto>> ChangeStatus(Guid id, ChangeStatusDto request)
    {
        Guid userId = GetCurrentUserId();

        Result<TaskResult> result = await _taskService.ChangeStatusAsync(id, request.Status, userId, request.Comment);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error),
                ErrorType.Forbidden => Forbidden(result.Error),
                ErrorType.Validation => BadRequest(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return Ok(MapToDto(result.Value));
    }

    [HttpPost("{id:guid}/assign")]
    public async Task<ActionResult<TaskDto>> Assign(Guid id, AssignTaskDto request)
    {
        Guid userId = GetCurrentUserId();

        Result<TaskResult> result = await _taskService.AssignTaskAsync(id, request.AssigneeId, userId);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error),
                ErrorType.Forbidden => Forbidden(result.Error),
                ErrorType.Validation => BadRequest(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return Ok(MapToDto(result.Value));
    }

    [HttpGet("projects/{projectId:guid}/statistics")]
    public async Task<ActionResult<TaskStatisticsDto>> GetStatistics(Guid projectId)
    {
        Guid userId = GetCurrentUserId();

        Result<TaskStatisticsResult> result = await _taskService.GetStatisticsAsync(projectId, userId);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error),
                ErrorType.Forbidden => Forbidden(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return Ok(
            new TaskStatisticsDto(
                result.Value.Total,
                result.Value.Todo,
                result.Value.InProgress,
                result.Value.Completed,
                result.Value.Cancelled));
    }

    private Guid GetCurrentUserId()
    {
        string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }

        return userId;
    }

    private ObjectResult Forbidden(Error error)
    {
        return new ObjectResult(error)
        {
            StatusCode = 403
        };
    }

    private static TaskDto MapToDto(TaskResult task)
    {
        return new TaskDto(
            task.Id,
            task.ProjectId,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.AssigneeId,
            null,
            task.DueDate,
            task.CreatedAt);
    }

    #endregion

}
