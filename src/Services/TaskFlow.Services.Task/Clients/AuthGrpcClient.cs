namespace TaskFlow.Services.Task.Clients;

using Auth;

using Grpc.Net.Client;

using Polly;
using Polly.Extensions.Http;

public interface IAuthGrpcClient
{

    #region Methods

    Task<GetUserResponse?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<string[]> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> CheckUserExistsAsync(Guid userId, CancellationToken cancellationToken = default);

    #endregion

}

public class AuthGrpcClient : IAuthGrpcClient
{

    #region Fields

    private readonly AuthService.AuthServiceClient _client;
    private readonly ILogger<AuthGrpcClient> _logger;
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;
    private readonly IAsyncPolicy<HttpResponseMessage> _circuitBreakerPolicy;

    #endregion

    #region Constructors

    public AuthGrpcClient(IConfiguration configuration, ILogger<AuthGrpcClient> logger)
    {
        _logger = logger;

        string address = configuration["Grpc:AuthService"] ?? "http://auth-service:8080";
        var httpHandler = new HttpClientHandler();
        GrpcChannel channel = GrpcChannel.ForAddress(
            address,
            new GrpcChannelOptions
            {
                HttpHandler = httpHandler
            });

        _client = new AuthService.AuthServiceClient(channel);

        // Configure resilience policies
        _retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                3,
                retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        _circuitBreakerPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    }

    #endregion

    #region Methods

    public async Task<GetUserResponse?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Calling AuthService.GetUser for UserId: {UserId}", userId);

            var request = new GetUserRequest
            {
                UserId = userId.ToString()
            };

            GetUserResponse? response = await _client.GetUserAsync(request, cancellationToken: cancellationToken);

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

    #endregion

}
