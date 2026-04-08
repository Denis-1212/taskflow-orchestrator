namespace TaskFlow.Services.Project.Clients;

using Auth;

using Grpc.Net.Client;

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

            var handler = new HttpClientHandler();
            var httpClient = new HttpClient(handler);

            GrpcChannel channel = GrpcChannel.ForAddress(
                "http://localhost:5007",
                new GrpcChannelOptions
                {
                    HttpClient = httpClient
                });

            var client = new AuthService.AuthServiceClient(channel);

            GetUserResponse response = await client.GetUserAsync(request, cancellationToken: cancellationToken);
            _logger.LogInformation("Response received. UserId: {UserId}, Email: {Email}", response.UserId, response.Email);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling AuthService.GetUser for UserId: {UserId}", userId);
            throw;
        }
    }

    #endregion

}
