namespace TaskFlow.Services.Task.Clients;

using Project;

public interface IProjectGrpcClient
{

    #region Methods

    Task<GetProjectResponse?> GetProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<bool> ProjectExistsAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<(bool IsMember, string Role)> ValidateMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);

    #endregion

}
