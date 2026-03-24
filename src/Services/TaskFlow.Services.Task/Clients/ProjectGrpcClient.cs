namespace TaskFlow.Services.Task.Clients;

using Grpc.Net.Client;

using Project;

public interface IProjectGrpcClient
{

    #region Methods

    Task<GetProjectResponse?> GetProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<bool> ProjectExistsAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<(bool IsMember, string Role)> ValidateMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);

    #endregion

}

public class ProjectGrpcClient : IProjectGrpcClient
{

    #region Fields

    private readonly ProjectService.ProjectServiceClient _client;
    private readonly ILogger<ProjectGrpcClient> _logger;

    #endregion

    #region Constructors

    public ProjectGrpcClient(IConfiguration configuration, ILogger<ProjectGrpcClient> logger)
    {
        _logger = logger;

        string address = configuration["Grpc:ProjectService"] ?? "http://project-service:8080";
        GrpcChannel channel = GrpcChannel.ForAddress(address);

        _client = new ProjectService.ProjectServiceClient(channel);
    }

    #endregion

    #region Methods

    public async Task<GetProjectResponse?> GetProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Calling ProjectService.GetProject for ProjectId: {ProjectId}", projectId);

            var request = new GetProjectRequest
            {
                ProjectId = projectId.ToString()
            };

            GetProjectResponse? response = await _client.GetProjectAsync(request, cancellationToken: cancellationToken);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling ProjectService.GetProject for ProjectId: {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<bool> ProjectExistsAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Calling ProjectService.ProjectExists for ProjectId: {ProjectId}", projectId);

            var request = new ProjectExistsRequest
            {
                ProjectId = projectId.ToString()
            };

            ProjectExistsResponse? response = await _client.ProjectExistsAsync(request, cancellationToken: cancellationToken);

            return response.Exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling ProjectService.ProjectExists for ProjectId: {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<(bool IsMember, string Role)> ValidateMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Calling ProjectService.ValidateMember for ProjectId: {ProjectId}, UserId: {UserId}",
                projectId,
                userId);

            var request = new ValidateMemberRequest
            {
                ProjectId = projectId.ToString(),
                UserId = userId.ToString()
            };

            ValidateMemberResponse? response = await _client.ValidateMemberAsync(request, cancellationToken: cancellationToken);

            return (response.IsMember, response.Role);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error calling ProjectService.ValidateMember for ProjectId: {ProjectId}, UserId: {UserId}",
                projectId,
                userId);

            throw;
        }
    }

    #endregion

}
