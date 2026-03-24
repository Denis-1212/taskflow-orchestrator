namespace TaskFlow.Services.Auth.Domain;

using System.Security.Cryptography;

public class RefreshToken
{

    #region Properties

    public string Token { get; private set; }
    public Guid UserId { get; private set; }
    public string DeviceId { get; private set; }
    public DateTime ExpiresAt { get; }
    public bool IsRevoked { get; private set; }

    #endregion

    #region Constructors

    public RefreshToken(Guid userId, string deviceId, TimeSpan expiration)
    {
        Token = GenerateSecureToken();
        UserId = userId;
        DeviceId = deviceId;
        ExpiresAt = DateTime.UtcNow.Add(expiration);
        IsRevoked = false;
    }

    private RefreshToken()
    {
        Token = string.Empty;
        DeviceId = string.Empty;
    }

    #endregion

    #region Methods

    public void Revoke()
    {
        IsRevoked = true;
    }

    public bool IsValid()
    {
        return !IsRevoked && ExpiresAt > DateTime.UtcNow;
    }

    private static string GenerateSecureToken()
    {
        byte[] bytes = new byte[32];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }

        return Convert.ToBase64String(bytes);
    }

    #endregion

}
