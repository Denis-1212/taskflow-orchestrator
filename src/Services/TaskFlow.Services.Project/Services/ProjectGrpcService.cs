namespace TaskFlow.Services.Project.Services;

using global::Project;

using Grpc.Core;

public class ProjectGrpcService : ProjectService.ProjectServiceBase
{

    #region Fields

    private readonly ILogger<ProjectGrpcService> _logger;

    #endregion

    #region Constructors

    public ProjectGrpcService(ILogger<ProjectGrpcService> logger)
    {
        _logger = logger;
    }

    #endregion

    #region Methods

    public override Task<GetProjectResponse> GetProject(GetProjectRequest request, ServerCallContext context)
    {
        _logger.LogInformation("GetProject called with ProjectId: {ProjectId}", request.ProjectId);

        var response = new GetProjectResponse
        {
            ProjectId = request.ProjectId,
            Name = "Test Project",
            Description = "Test Description",
            OwnerId = Guid.NewGuid().ToString(),
            IsDeleted = false
        };

        return Task.FromResult(response);
    }

    public override Task<ProjectExistsResponse> ProjectExists(ProjectExistsRequest request, ServerCallContext context)
    {
        _logger.LogInformation("ProjectExists called for ProjectId: {ProjectId}", request.ProjectId);


        var response = new ProjectExistsResponse
        {
            Exists = true
        };

        return Task.FromResult(response);
    }

    public override Task<ValidateMemberResponse> ValidateMember(ValidateMemberRequest request, ServerCallContext context)
    {
        _logger.LogInformation(
            "ValidateMember called for ProjectId: {ProjectId}, UserId: {UserId}",
            request.ProjectId,
            request.UserId);

        
        var response = new ValidateMemberResponse
        {
            IsMember = true,
            Role = "Member"
        };

        return Task.FromResult(response);
    }

    public override Task<GetUserProjectsResponse> GetUserProjects(GetUserProjectsRequest request, ServerCallContext context)
    {
        _logger.LogInformation("GetUserProjects called for UserId: {UserId}", request.UserId);

        var response = new GetUserProjectsResponse();

        var userProject = new UserProject
        {
            ProjectId = Guid.NewGuid().ToString(),
            Name = "Test Project",
            Role = "Member"
        };

        response.Projects.Add(userProject);

        return Task.FromResult(response);
    }

    #endregion

}
