namespace TaskFlow.Services.Auth.Services;

using Domain;

public interface IJwtService
{

    #region Methods

    string GenerateAccessToken(User user);
    string GenerateRefreshToken();

    #endregion

}
