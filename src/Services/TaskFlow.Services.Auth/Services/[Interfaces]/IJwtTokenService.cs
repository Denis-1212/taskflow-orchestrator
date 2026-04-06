namespace TaskFlow.Services.Auth.Services;

using Domain;

public interface IJwtTokenService
{

    #region Methods

    string GenerateAccessToken(User user);
    RefreshToken GenerateRefreshToken(Guid userId, string deviceId);

    #endregion

}
