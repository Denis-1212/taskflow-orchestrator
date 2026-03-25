namespace TaskFlow.Services.Project.Application.Services;

using Domain;

using Infrastructure;

using Microsoft.EntityFrameworkCore;

using Shared.Kernel;

using Project = Domain.Project;

public class ProjectService(ProjectDbContext context, ILogger<ProjectService> logger) : IProjectService
{

    #region Methods

    public async Task<Result<ProjectResult>> CreateAsync(string name, string description, Guid ownerId)
    {
        logger.LogInformation("Creating project {Name} for user {OwnerId}", name, ownerId);

        var project = new Project(name, description, ownerId);

        context.Projects.Add(project);
        await context.SaveChangesAsync();

        logger.LogInformation("Project created with Id: {ProjectId}", project.Id);

        return MapToResult(project);
    }

    public async Task<Result<ProjectResult>> UpdateAsync(Guid projectId, string name, string description, Guid userId)
    {
        logger.LogInformation("Updating project {ProjectId} by user {UserId}", projectId, userId);

        Project? project = await context.Projects
                               .Include(p => p.Members)
                               .FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted);

        if (project == null)
        {
            return Error.NotFound("Project", projectId);
        }

        ProjectRole? role = project.GetMemberRole(userId);

        if (role != ProjectRole.Owner)
        {
            return Error.Forbidden("Only project owner can update project");
        }

        project.Update(name, description);
        await context.SaveChangesAsync();

        return MapToResult(project);
    }

    public async Task<Result> DeleteAsync(Guid projectId, Guid userId)
    {
        logger.LogInformation("Deleting project {ProjectId} by user {UserId}", projectId, userId);

        Project? project = await context.Projects
                               .Include(p => p.Members)
                               .FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted);

        if (project == null)
        {
            return Result.Failure(Error.NotFound("Project", projectId));
        }

        ProjectRole? role = project.GetMemberRole(userId);

        if (role != ProjectRole.Owner)
        {
            return Result.Failure(Error.Forbidden("Only project owner can delete project"));
        }

        project.SoftDelete();
        await context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<ProjectResult>> GetByIdAsync(Guid projectId, Guid userId)
    {
        Project? project = await context.Projects
                               .Include(p => p.Members)
                               .FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted);

        if (project == null)
        {
            return Error.NotFound("Project", projectId);
        }

        if (!project.IsMember(userId))
        {
            return Error.Forbidden("User is not a member of this project");
        }

        return MapToResult(project);
    }

    public async Task<Result<IEnumerable<ProjectResult>>> GetUserProjectsAsync(Guid userId, bool includeDeleted = false)
    {
        IQueryable<Project> query = context.Projects
            .Include(p => p.Members)
            .Where(p => p.Members.Any(m => m.UserId == userId));

        if (!includeDeleted)
        {
            query = query.Where(p => !p.IsDeleted);
        }

        List<Project> projects = await query.ToListAsync();

        return projects.Select(MapToResult).ToList();
    }

    public async Task<Result> AddMemberAsync(Guid projectId, Guid userId, string role, Guid addedBy)
    {
        logger.LogInformation("Adding user {UserId} to project {ProjectId} by {AddedBy}", userId, projectId, addedBy);

        if (!Enum.TryParse(role, true, out ProjectRole projectRole))
        {
            return Result.Failure(Error.Validation($"Invalid role: {role}. Valid roles: Owner, Member, Viewer"));
        }

        Project? project = await context.Projects
                               .Include(p => p.Members)
                               .FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted);

        if (project == null)
        {
            return Result.Failure(Error.NotFound("Project", projectId));
        }

        Result result = project.AddMember(userId, projectRole, addedBy);

        if (result.IsFailure)
        {
            return result;
        }

        await context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> RemoveMemberAsync(Guid projectId, Guid userId, Guid removedBy)
    {
        logger.LogInformation("Removing user {UserId} from project {ProjectId} by {RemovedBy}", userId, projectId, removedBy);

        Project? project = await context.Projects
                               .Include(p => p.Members)
                               .FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted);

        if (project == null)
        {
            return Result.Failure(Error.NotFound("Project", projectId));
        }

        Result result = project.RemoveMember(userId, removedBy);

        if (result.IsFailure)
        {
            return result;
        }

        await context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> UpdateMemberRoleAsync(Guid projectId, Guid userId, string newRole, Guid updatedBy)
    {
        logger.LogInformation("Updating role for user {UserId} in project {ProjectId} by {UpdatedBy}", userId, projectId, updatedBy);

        if (!Enum.TryParse(newRole, true, out ProjectRole projectRole))
        {
            return Result.Failure(Error.Validation($"Invalid role: {newRole}. Valid roles: Owner, Member, Viewer"));
        }

        Project? project = await context.Projects
                               .Include(p => p.Members)
                               .FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted);

        if (project == null)
        {
            return Result.Failure(Error.NotFound("Project", projectId));
        }

        Result result = project.UpdateMemberRole(userId, projectRole, updatedBy);

        if (result.IsFailure)
        {
            return result;
        }

        await context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<IEnumerable<ProjectMemberResult>>> GetProjectMembersAsync(Guid projectId, Guid userId)
    {
        Project? project = await context.Projects
                               .Include(p => p.Members)
                               .FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted);

        if (project == null)
        {
            return Error.NotFound("Project", projectId);
        }

        if (!project.IsMember(userId))
        {
            return Error.Forbidden("User is not a member of this project");
        }

        IEnumerable<ProjectMemberResult> members = project.Members.Select(m => new ProjectMemberResult(
            m.UserId,
            m.Role.ToString(),
            m.JoinedAt));

        return members.ToList();
    }

    // gRPC методы
    public async Task<Result<ProjectResult>> GetProjectForGrpcAsync(Guid projectId)
    {
        Project? project = await context.Projects
                               .FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted);

        if (project == null)
        {
            return Error.NotFound("Project", projectId);
        }

        return MapToResult(project);
    }

    public async Task<Result<bool>> ProjectExistsAsync(Guid projectId)
    {
        bool exists = await context.Projects
                          .AnyAsync(p => p.Id == projectId && !p.IsDeleted);

        return exists;
    }

    public async Task<Result<MemberValidationResult>> ValidateMemberAsync(Guid projectId, Guid userId)
    {
        Project? project = await context.Projects
                               .Include(p => p.Members)
                               .FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted);

        if (project == null)
        {
            return Error.NotFound("Project", projectId);
        }

        ProjectRole? role = project.GetMemberRole(userId);
        bool isMember = role.HasValue;

        return new MemberValidationResult(isMember, role?.ToString() ?? string.Empty);
    }

    private static ProjectResult MapToResult(Project project)
    {
        return new ProjectResult(
            project.Id,
            project.Name,
            project.Description,
            project.OwnerId,
            project.IsDeleted,
            project.CreatedAt,
            project.UpdatedAt);
    }

    #endregion

}
