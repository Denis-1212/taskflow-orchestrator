namespace TaskFlow.Services.Auth.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Domain;

using Microsoft.IdentityModel.Tokens;

public class JwtService(IConfiguration configuration, ILogger<JwtService> logger) : IJwtService
{

    #region Methods

    public string GenerateAccessToken(User user)
    {
        string? secret = configuration["Jwt:Secret"];

        if (string.IsNullOrEmpty(secret))
        {
            throw new InvalidOperationException("JWT Secret is not configured");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        string? expirationMinutes = configuration["Jwt:AccessTokenExpirationMinutes"];

        if (string.IsNullOrEmpty(expirationMinutes))
        {
            expirationMinutes = "30";
            logger.LogWarning("Jwt:AccessTokenExpirationMinutes not configured, using default: 30");
        }

        var token = new JwtSecurityToken(
            configuration["Jwt:Issuer"] ?? "TaskFlow",
            configuration["Jwt:Audience"] ?? "TaskFlow",
            claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(expirationMinutes)),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        byte[] randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    #endregion

}
