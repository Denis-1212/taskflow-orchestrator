namespace TaskFlow.Services.Task.Clients;

using Auth;

using Grpc.Net.Client;

using Notification.Clients;

public class AuthGrpcClient : IAuthGrpcClient
{

    #region Fields

    private readonly AuthService.AuthServiceClient _client;
    private readonly ILogger<AuthGrpcClient> _logger;

    #endregion

    #region Constructors

    public AuthGrpcClient(IConfiguration configuration, ILogger<AuthGrpcClient> logger)
    {
        string url = configuration["Grpc:AuthService:Url"] ?? "http://localhost:5007";
        _logger = logger;
        _logger.LogInformation("Creating gRPC client for AuthService at {Url}", url);

        GrpcChannel channel = GrpcChannel.ForAddress(url);
        _client = new AuthService.AuthServiceClient(channel);
    }

    #endregion

    #region Methods

    public async Task<GetUserResponse> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Calling AuthService.GetUser for UserId: {UserId}", userId);

            var request = new GetUserRequest
            {
                UserId = userId.ToString()
            };

            GetUserResponse response = await _client.GetUserAsync(request, cancellationToken: cancellationToken);
            _logger.LogInformation("Response received. UserId: {UserId}, Email: {Email}", response.UserId, response.Email);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling AuthService.GetUser for UserId: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Calling AuthService.ValidateToken");

            var request = new ValidateTokenRequest
            {
                Token = token
            };

            ValidateTokenResponse? response = await _client.ValidateTokenAsync(request, cancellationToken: cancellationToken);

            return response.IsValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling AuthService.ValidateToken");
            throw;
        }
    }

    public async Task<string[]> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Calling AuthService.GetUserRoles for UserId: {UserId}", userId);

            var request = new GetUserRolesRequest
            {
                UserId = userId.ToString()
            };

            GetUserRolesResponse? response = await _client.GetUserRolesAsync(request, cancellationToken: cancellationToken);

            return response.Roles.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling AuthService.GetUserRoles for UserId: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> CheckUserExistsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Calling AuthService.CheckUserExists for UserId: {UserId}", userId);

            var request = new CheckUserExistsRequest
            {
                UserId = userId.ToString()
            };

            CheckUserExistsResponse? response = await _client.CheckUserExistsAsync(request, cancellationToken: cancellationToken);

            return response.Exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling AuthService.CheckUserExists for UserId: {UserId}", userId);
            throw;
        }
    }

    public async Task<GetUserResponse> GetUserByEmailAsync(string email)
    {
        var request = new GetUserByEmailRequest
        {
            Email = email
        };

        GetUserResponse? response = await _client.GetUserByEmailAsync(request);
        return response;
    }

    #endregion

}
