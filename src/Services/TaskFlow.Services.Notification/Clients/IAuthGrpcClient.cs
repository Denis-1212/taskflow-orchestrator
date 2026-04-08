namespace TaskFlow.Services.Notification.Clients;

using Auth;

public interface IAuthGrpcClient
{

    #region Methods

    Task<GetUserResponse> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);

    #endregion

}
