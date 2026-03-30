namespace TaskFlow.Services.Auth.Services;

using System.Text.Json;

using Domain;

using StackExchange.Redis;

public class RefreshTokenService(IConnectionMultiplexer redis, ILogger<RefreshTokenService> logger) : IRefreshTokenService
{

    #region Properties

    private IDatabase Db => redis.GetDatabase();

    #endregion

    #region Methods

    public async Task SaveRefreshTokenAsync(string token, Guid userId, string ipAddress, TimeSpan ttl)
    {
        var data = new RefreshTokenData
        {
            UserId = userId,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };

        string json = JsonSerializer.Serialize(data);

        await Db.StringSetAsync(GetKey(token), json, ttl);
        logger.LogDebug("Refresh token saved for user {UserId}, expires in {Ttl}", userId, ttl);
    }

    public async Task<RefreshTokenData?> GetRefreshTokenAsync(string token)
    {
        RedisValue json = await Db.StringGetAsync(GetKey(token));

        if (json.IsNullOrEmpty)
        {
            logger.LogDebug("Refresh token not found or expired");
            return null;
        }

        return JsonSerializer.Deserialize<RefreshTokenData>(((string)json)!);
    }

    public async Task RemoveRefreshTokenAsync(string token)
    {
        await Db.KeyDeleteAsync(GetKey(token));
        logger.LogDebug("Refresh token removed");
    }

    private static string GetKey(string token)
    {
        return $"refresh_token:{token}";
    }

    #endregion

}
