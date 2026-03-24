namespace TaskFlow.Services.Auth.Services;

using global::Auth;

using Grpc.Core;

public class AuthGrpcService : AuthService.AuthServiceBase
{

    #region Fields

    private readonly ILogger<AuthGrpcService> _logger;

    #endregion

    #region Constructors

    public AuthGrpcService(ILogger<AuthGrpcService> logger)
    {
        _logger = logger;
    }

    #endregion

    #region Methods

    public override Task<GetUserResponse> GetUser(GetUserRequest request, ServerCallContext context)
    {
        _logger.LogInformation("GetUser called with UserId: {UserId}", request.UserId);

        var response = new GetUserResponse
        {
            UserId = request.UserId,
            Email = "user@example.com",
            FullName = "Test User",
            IsActive = true
        };

        return Task.FromResult(response);
    }

    public override Task<ValidateTokenResponse> ValidateToken(ValidateTokenRequest request, ServerCallContext context)
    {
        _logger.LogInformation("ValidateToken called");

        var response = new ValidateTokenResponse
        {
            IsValid = true,
            UserId = Guid.NewGuid().ToString(),
            Email = "user@example.com"
        };

        return Task.FromResult(response);
    }

    public override Task<GetUserRolesResponse> GetUserRoles(GetUserRolesRequest request, ServerCallContext context)
    {
        _logger.LogInformation("GetUserRoles called for UserId: {UserId}", request.UserId);

        var response = new GetUserRolesResponse();
        response.Roles.Add("User");

        return Task.FromResult(response);
    }

    public override Task<CheckUserExistsResponse> CheckUserExists(CheckUserExistsRequest request, ServerCallContext context)
    {
        _logger.LogInformation("CheckUserExists called for UserId: {UserId}", request.UserId);

        var response = new CheckUserExistsResponse
        {
            Exists = true
        };

        return Task.FromResult(response);
    }

    #endregion

}
