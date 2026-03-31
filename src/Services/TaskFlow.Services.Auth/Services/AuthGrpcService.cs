namespace TaskFlow.Services.Auth.Services;

using Application.Services;

using global::Auth;

using Grpc.Core;

using Shared.Kernel;

using AuthService = global::Auth.AuthService;

public class AuthGrpcService(IAuthService authService, ILogger<AuthGrpcService> logger) : AuthService.AuthServiceBase
{

    #region Methods

    public override async Task<GetUserResponse> GetUser(GetUserRequest request, ServerCallContext context)
    {
        logger.LogInformation("gRPC GetUser called for UserId: {UserId}", request.UserId);

        if (!Guid.TryParse(request.UserId, out Guid userId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user ID format"));
        }

        Result<UserResult> result = await authService.GetUserByIdAsync(userId);

        if (result.IsFailure)
        {
            throw new RpcException(new Status(StatusCode.NotFound, result.Error!.Message));
        }

        var response = new GetUserResponse
        {
            UserId = result.Value.Id.ToString(),
            Email = result.Value.Email,
            FullName = result.Value.FullName,
            IsActive = result.Value.IsActive
        };

        // Добавляем роли
        if (result.Value.Roles.Any())
        {
            response.Roles.AddRange(result.Value.Roles);
        }

        logger.LogInformation(
            "GetUser response: UserId={UserId}, Email={Email}, FullName={FullName}, RolesCount={RolesCount}",
            response.UserId,
            response.Email,
            response.FullName,
            response.Roles.Count);

        return response;
    }

    public override async Task<GetUserResponse> GetUserByEmail(GetUserByEmailRequest request, ServerCallContext context)
    {
        logger.LogInformation("gRPC GetUserByEmail called for Email: {Email}", request.Email);

        Result<UserResult> result = await authService.GetUserByEmailAsync(request.Email);

        if (result.IsFailure)
        {
            throw new RpcException(new Status(StatusCode.NotFound, result.Error!.Message));
        }

        return new GetUserResponse
        {
            UserId = result.Value.Id.ToString(),
            Email = result.Value.Email,
            FullName = result.Value.FullName,
            IsActive = result.Value.IsActive
        };
    }

    public override async Task<ValidateTokenResponse> ValidateToken(ValidateTokenRequest request, ServerCallContext context)
    {
        logger.LogInformation("ValidateToken called");
        bool result = await authService.ValidateToken(request.Token);

        return new ValidateTokenResponse
        {
            IsValid = result
        };
    }

    public override async Task<GetUserRolesResponse> GetUserRoles(GetUserRolesRequest request, ServerCallContext context)
    {
        logger.LogInformation("GetUserRoles called for UserId: {UserId}", request.UserId);
        Guid.TryParse(request.UserId, out Guid userId);
        Result<string[]> roles = await authService.GetUserRoles(userId);

        var response = new GetUserRolesResponse();
        response.Roles.AddRange(Array.ConvertAll(roles.Value, item => item.ToString()));

        return response;
    }

    public override async Task<CheckUserExistsResponse> CheckUserExists(CheckUserExistsRequest request, ServerCallContext context)
    {
        logger.LogInformation("CheckUserExists called for UserId: {UserId}", request.UserId);

        if (!Guid.TryParse(request.UserId, out Guid userId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user ID format"));
        }

        Result<UserResult> result = await authService.GetUserByIdAsync(userId);

        if (result.IsFailure)
        {
            throw new RpcException(new Status(StatusCode.NotFound, result.Error!.Message));
        }

        return new CheckUserExistsResponse
        {
            Exists = result.Value.Id.ToString() == request.UserId
        };
    }

    #endregion

}
