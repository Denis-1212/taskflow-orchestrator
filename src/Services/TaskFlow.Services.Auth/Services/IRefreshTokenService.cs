namespace TaskFlow.Services.Auth.Services;

using Domain;

public interface IRefreshTokenService
{

    #region Methods

    Task SaveRefreshTokenAsync(string token, Guid userId, string ipAddress, TimeSpan ttl);
    Task<RefreshTokenData?> GetRefreshTokenAsync(string token);
    Task RemoveRefreshTokenAsync(string token);

    #endregion

}
