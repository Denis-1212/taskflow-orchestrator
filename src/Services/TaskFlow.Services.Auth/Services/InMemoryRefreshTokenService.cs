namespace TaskFlow.Services.Auth.Services;

using System.Collections.Concurrent;

using Domain;

public class InMemoryRefreshTokenService(ILogger<InMemoryRefreshTokenService> logger) : IRefreshTokenService
{

    #region Fields

    private readonly ConcurrentDictionary<string, RefreshTokenData> _tokens = new();

    #endregion

    #region Methods

    public Task SaveRefreshTokenAsync(string token, Guid userId, string ipAddress, TimeSpan ttl)
    {
        _tokens[token] = new RefreshTokenData
        {
            UserId = userId,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };

        logger.LogDebug("Refresh token saved for user {UserId}", userId);
        return Task.CompletedTask;
    }

    public Task<RefreshTokenData?> GetRefreshTokenAsync(string token)
    {
        _tokens.TryGetValue(token, out RefreshTokenData? data);
        return Task.FromResult(data);
    }

    public Task RemoveRefreshTokenAsync(string token)
    {
        _tokens.TryRemove(token, out _);
        logger.LogDebug("Refresh token removed");
        return Task.CompletedTask;
    }

    #endregion

}
