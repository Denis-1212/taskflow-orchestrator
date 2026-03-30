namespace TaskFlow.Services.Auth.Services;

using global::Auth;

using Grpc.Core;

public class AuthGrpcService(ILogger<AuthGrpcService> logger)
{

    #region Methods

    public Task<GetUserResponse> GetUser(GetUserRequest request, ServerCallContext context)
    {
        logger.LogInformation("GetUser called with UserId: {UserId}", request.UserId);

        var response = new GetUserResponse
        {
            UserId = request.UserId,
            Email = "user@example.com",
            FullName = "Test User",
            IsActive = true
        };

        return Task.FromResult(response);
    }

    public Task<ValidateTokenResponse> ValidateToken(ValidateTokenRequest request, ServerCallContext context)
    {
        logger.LogInformation("ValidateToken called");

        var response = new ValidateTokenResponse
        {
            IsValid = true,
            UserId = Guid.NewGuid().ToString(),
            Email = "user@example.com"
        };

        return Task.FromResult(response);
    }

    public Task<GetUserRolesResponse> GetUserRoles(GetUserRolesRequest request, ServerCallContext context)
    {
        logger.LogInformation("GetUserRoles called for UserId: {UserId}", request.UserId);

        var response = new GetUserRolesResponse();
        response.Roles.Add("User");

        return Task.FromResult(response);
    }

    public Task<CheckUserExistsResponse> CheckUserExists(CheckUserExistsRequest request, ServerCallContext context)
    {
        logger.LogInformation("CheckUserExists called for UserId: {UserId}", request.UserId);

        var response = new CheckUserExistsResponse
        {
            Exists = true
        };

        return Task.FromResult(response);
    }

    #endregion

}
