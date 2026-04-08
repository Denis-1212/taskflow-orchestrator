namespace TaskFlow.Services.Task.Clients;

public interface IProjectGrpcClient
{

    #region Methods

    Task<bool> ProjectExistsAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<(bool IsMember, string Role)> ValidateMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);

    #endregion

}
