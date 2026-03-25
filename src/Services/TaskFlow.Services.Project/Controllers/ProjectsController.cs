namespace TaskFlow.Services.Project.Controllers;

using System.Security.Claims;

using Application.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Shared.DTOs;
using Shared.Kernel;

[ApiController]
[Authorize]
[Route("api/projects")]
public class ProjectsController(IProjectService projectService) : ControllerBase
{

    #region Methods

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> Create(CreateProjectDto request)
    {
        Guid userId = GetCurrentUserId();

        Result<ProjectResult> result = await projectService.CreateAsync(request.Name, request.Description, userId);

        if (result.IsFailure)
        {
            return BadRequest(result.Error!);
        }

        return Ok(MapToDto(result.Value));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectDto>> GetById(Guid id)
    {
        Guid userId = GetCurrentUserId();

        Result<ProjectResult> result = await projectService.GetByIdAsync(id, userId);

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
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetUserProjects([FromQuery] bool includeDeleted = false)
    {
        Guid userId = GetCurrentUserId();

        Result<IEnumerable<ProjectResult>> result = await projectService.GetUserProjectsAsync(userId, includeDeleted);

        if (result.IsFailure)
        {
            return BadRequest(result.Error!);
        }

        return Ok(result.Value.Select(MapToDto));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateProjectDto request)
    {
        Guid userId = GetCurrentUserId();

        Result<ProjectResult> result = await projectService.UpdateAsync(id, request.Name, request.Description, userId);

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

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        Guid userId = GetCurrentUserId();

        Result result = await projectService.DeleteAsync(id, userId);

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

    [HttpGet("{id:guid}/members")]
    public async Task<ActionResult<IEnumerable<ProjectMemberDto>>> GetMembers(Guid id)
    {
        Guid userId = GetCurrentUserId();

        Result<IEnumerable<ProjectMemberResult>> result = await projectService.GetProjectMembersAsync(id, userId);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error),
                ErrorType.Forbidden => Forbidden(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return Ok(result.Value.Select(MapToMemberDto));
    }

    [HttpPost("{id:guid}/members")]
    public async Task<IActionResult> AddMember(Guid id, AddProjectMemberDto request)
    {
        Guid userId = GetCurrentUserId();

        Result result = await projectService.AddMemberAsync(id, request.UserId, request.Role, userId);

        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => NotFound(result.Error),
                ErrorType.Forbidden => Forbidden(result.Error),
                ErrorType.Conflict => Conflict(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}/members/{memberId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid memberId)
    {
        Guid userId = GetCurrentUserId();

        Result result = await projectService.RemoveMemberAsync(id, memberId, userId);

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

    [HttpPut("{id:guid}/members/{memberId:guid}/role")]
    public async Task<IActionResult> UpdateMemberRole(Guid id, Guid memberId, [FromBody] string role)
    {
        Guid userId = GetCurrentUserId();

        Result result = await projectService.UpdateMemberRoleAsync(id, memberId, role, userId);

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

    private Guid GetCurrentUserId()
    {
        string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }

        return userId;
    }

    private static ProjectDto MapToDto(ProjectResult project)
    {
        return new ProjectDto(
            project.Id,
            project.Name,
            project.Description,
            project.OwnerId,
            project.CreatedAt);
    }

    private static ProjectMemberDto MapToMemberDto(ProjectMemberResult member)
    {
        return new ProjectMemberDto(
            member.UserId,
            string.Empty, // Email - будет подтягиваться из Auth Service при необходимости
            string.Empty, // FullName - будет подтягиваться из Auth Service при необходимости
            member.Role);
    }

    private static ObjectResult Forbidden(Error error)
    {
        return new ObjectResult(error)
        {
            StatusCode = 403
        };
    }

    #endregion

}
