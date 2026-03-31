namespace TaskFlow.Services.Task.Clients;

using Auth;

public interface IAuthGrpcClient
{

    #region Methods

    Task<GetUserResponse> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<string[]> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> CheckUserExistsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<GetUserResponse> GetUserByEmailAsync(string email);

    #endregion

}
