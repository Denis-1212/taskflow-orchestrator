namespace TaskFlow.Services.Project.Services;

using Application.Services;

using global::Project;

using Grpc.Core;

using Shared.Kernel;

using ProjectService = global::Project.ProjectService;

public class ProjectGrpcService : ProjectService.ProjectServiceBase
{

    #region Fields

    private readonly IProjectService _projectService;
    private readonly ILogger<ProjectGrpcService> _logger;

    #endregion

    #region Constructors

    public ProjectGrpcService(IProjectService projectService, ILogger<ProjectGrpcService> logger)
    {
        _projectService = projectService;
        _logger = logger;
    }

    #endregion

    #region Methods

    public override async Task<GetProjectResponse> GetProject(GetProjectRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC GetProject called for ProjectId: {ProjectId}", request.ProjectId);

        if (!Guid.TryParse(request.ProjectId, out Guid projectId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid project ID format"));
        }

        Result<ProjectResult> result = await _projectService.GetProjectForGrpcAsync(projectId);

        if (result.IsFailure)
        {
            throw new RpcException(new Status(StatusCode.NotFound, result.Error!.Message));
        }

        return new GetProjectResponse
        {
            ProjectId = result.Value.Id.ToString(),
            Name = result.Value.Name,
            Description = result.Value.Description,
            OwnerId = result.Value.OwnerId.ToString(),
            IsDeleted = result.Value.IsDeleted
        };
    }

    public override async Task<ProjectExistsResponse> ProjectExists(ProjectExistsRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC ProjectExists called for ProjectId: {ProjectId}", request.ProjectId);

        if (!Guid.TryParse(request.ProjectId, out Guid projectId))
        {
            return new ProjectExistsResponse
            {
                Exists = false
            };
        }

        Result<bool> result = await _projectService.ProjectExistsAsync(projectId);

        return new ProjectExistsResponse
        {
            Exists = result.IsSuccess && result.Value
        };
    }

    public override async Task<ValidateMemberResponse> ValidateMember(ValidateMemberRequest request, ServerCallContext context)
    {
        _logger.LogInformation(
            "gRPC ValidateMember called for ProjectId: {ProjectId}, UserId: {UserId}",
            request.ProjectId,
            request.UserId);

        if (!Guid.TryParse(request.ProjectId, out Guid projectId) ||
            !Guid.TryParse(request.UserId, out Guid userId))
        {
            return new ValidateMemberResponse
            {
                IsMember = false,
                Role = string.Empty
            };
        }

        Result<MemberValidationResult> result = await _projectService.ValidateMemberAsync(projectId, userId);

        if (result.IsFailure)
        {
            return new ValidateMemberResponse
            {
                IsMember = false,
                Role = string.Empty
            };
        }

        return new ValidateMemberResponse
        {
            IsMember = result.Value.IsMember,
            Role = result.Value.Role
        };
    }

    public override async Task<GetUserProjectsResponse> GetUserProjects(GetUserProjectsRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC GetUserProjects called for UserId: {UserId}", request.UserId);

        if (!Guid.TryParse(request.UserId, out Guid userId))
        {
            return new GetUserProjectsResponse();
        }

        Result<IEnumerable<ProjectResult>> result = await _projectService.GetUserProjectsAsync(userId);

        var response = new GetUserProjectsResponse();

        if (result.IsSuccess)
        {
            foreach (ProjectResult project in result.Value)
            {
                response.Projects.Add(
                    new UserProject
                    {
                        ProjectId = project.Id.ToString(),
                        Name = project.Name,
                        Role = "Member"
                    });
            }
        }

        return response;
    }

    #endregion

}
