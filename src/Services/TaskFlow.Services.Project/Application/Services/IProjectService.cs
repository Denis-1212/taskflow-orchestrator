namespace TaskFlow.Services.Project.Application.Services;

using Shared.Kernel;

public interface IProjectService
{

    #region Methods

    Task<Result<ProjectResult>> CreateAsync(string name, string description, Guid ownerId);
    Task<Result<ProjectResult>> UpdateAsync(Guid projectId, string name, string description, Guid userId);
    Task<Result> DeleteAsync(Guid projectId, Guid userId);
    Task<Result<ProjectResult>> GetByIdAsync(Guid projectId, Guid userId);
    Task<Result<IEnumerable<ProjectResult>>> GetUserProjectsAsync(Guid userId, bool includeDeleted = false);

    Task<Result> AddMemberAsync(Guid projectId, Guid userId, string role, Guid addedBy);
    Task<Result> RemoveMemberAsync(Guid projectId, Guid userId, Guid removedBy);
    Task<Result> UpdateMemberRoleAsync(Guid projectId, Guid userId, string newRole, Guid updatedBy);
    Task<Result<IEnumerable<ProjectMemberResult>>> GetProjectMembersAsync(Guid projectId, Guid userId);

    // Для gRPC
    Task<Result<ProjectResult>> GetProjectForGrpcAsync(Guid projectId);
    Task<Result<bool>> ProjectExistsAsync(Guid projectId);
    Task<Result<MemberValidationResult>> ValidateMemberAsync(Guid projectId, Guid userId);

    #endregion

}

public record ProjectResult(
    Guid Id,
    string Name,
    string Description,
    Guid OwnerId,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record ProjectMemberResult(
    Guid UserId,
    string Role,
    DateTime JoinedAt);

public record MemberValidationResult(
    bool IsMember,
    string Role);
