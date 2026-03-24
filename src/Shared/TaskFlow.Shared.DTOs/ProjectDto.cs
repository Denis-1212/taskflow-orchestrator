namespace TaskFlow.Shared.DTOs;

public record ProjectDto(Guid Id, string Name, string Description, Guid OwnerId, DateTime CreatedAt);
public record CreateProjectDto(string Name, string Description);
public record UpdateProjectDto(string Name, string Description);
public record ProjectMemberDto(Guid UserId, string UserEmail, string FullName, string Role);
public record AddProjectMemberDto(Guid UserId, string Role);
