namespace TaskFlow.Services.Auth.Services;

using Application.Services;

using Domain;

using Infrastructure;

using Microsoft.EntityFrameworkCore;

using Shared.Kernel;

public class AuthService(
    AuthDbContext context,
    IJwtService jwtService,
    IRefreshTokenService refreshTokenService,
    IPasswordHasher passwordHasher,
    ILogger<AuthService> logger)
    : IAuthService
{

    #region Methods

    public async Task<Result<UserResult>> GetCurrentUserAsync(Guid userId)
    {
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return Error.NotFound("User", userId);
        }

        if (!user.IsActive)
        {
            return Error.Unauthorized("Account is disabled");
        }

        return new UserResult(user.Id, user.Email, user.FullName, user.IsActive, user.Roles.ToArray());
    }

    public async Task<Result<AuthResult>> RegisterAsync(string email, string password, string fullName, string ipAddress)
    {
        logger.LogInformation("Registration attempt for email: {Email}", email);

        // Check if user exists
        User? existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (existingUser != null)
        {
            return Error.Conflict($"User with email {email} already exists");
        }

        // Create new user
        var user = new User(email, password, fullName);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Generate tokens
        string accessToken = jwtService.GenerateAccessToken(user);
        string refreshToken = jwtService.GenerateRefreshToken();

        // Save refresh token in Redis
        await refreshTokenService.SaveRefreshTokenAsync(
            refreshToken,
            user.Id,
            ipAddress,
            TimeSpan.FromDays(7));

        logger.LogInformation("User {UserId} registered successfully", user.Id);

        return new AuthResult(
            accessToken,
            refreshToken,
            new UserResult(user.Id, user.Email, user.FullName, user.IsActive, user.Roles.ToArray()));
    }

    public async Task<Result<AuthResult>> LoginAsync(string email, string password, string ipAddress)
    {
        logger.LogInformation("Login attempt for email: {Email}", email);

        User? user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || !user.VerifyPassword(password, passwordHasher))
        {
            return Error.Unauthorized("Invalid email or password");
        }

        if (!user.IsActive)
        {
            return Error.Unauthorized("Account is disabled");
        }

        string accessToken = jwtService.GenerateAccessToken(user);
        string refreshToken = jwtService.GenerateRefreshToken();

        // Save refresh token in Redis
        await refreshTokenService.SaveRefreshTokenAsync(
            refreshToken,
            user.Id,
            ipAddress,
            TimeSpan.FromDays(7));

        logger.LogInformation("User {UserId} logged in successfully", user.Id);

        return new AuthResult(
            accessToken,
            refreshToken,
            new UserResult(user.Id, user.Email, user.FullName, user.IsActive, user.Roles.ToArray()));
    }

    public async Task<Result<AuthResult>> RefreshTokenAsync(string refreshToken, string ipAddress)
    {
        logger.LogInformation("Refresh token attempt");

        // Get from Redis
        RefreshTokenData? tokenData = await refreshTokenService.GetRefreshTokenAsync(refreshToken);

        if (tokenData == null)
        {
            return Error.Unauthorized("Invalid or expired refresh token");
        }

        User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == tokenData.UserId);

        if (user == null || !user.IsActive)
        {
            return Error.Unauthorized("User not found or inactive");
        }

        // Delete old refresh token
        await refreshTokenService.RemoveRefreshTokenAsync(refreshToken);

        // Generate new token pair
        string newAccessToken = jwtService.GenerateAccessToken(user);
        string newRefreshToken = jwtService.GenerateRefreshToken();

        // Save new refresh token
        await refreshTokenService.SaveRefreshTokenAsync(
            newRefreshToken,
            user.Id,
            ipAddress,
            TimeSpan.FromDays(7));

        logger.LogInformation("Token refreshed for user {UserId}", user.Id);

        return new AuthResult(
            newAccessToken,
            newRefreshToken,
            new UserResult(user.Id, user.Email, user.FullName, user.IsActive, user.Roles.ToArray()));
    }

    public async Task<Result> LogoutAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Result.Success();
        }

        await refreshTokenService.RemoveRefreshTokenAsync(refreshToken);
        logger.LogInformation("User logged out, refresh token removed");

        return Result.Success();
    }

    public async Task<Result<UserResult>> GetUserByIdAsync(Guid userId)
    {
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return Error.NotFound("User", userId);
        }

        return new UserResult(user.Id, user.Email, user.FullName, user.IsActive, user.Roles.ToArray());
    }

    public async Task<Result<UserResult>> GetUserByEmailAsync(string email)
    {
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            return Error.NotFound("User", email);
        }

        return new UserResult(user.Id, user.Email, user.FullName, user.IsActive, user.Roles.ToArray());
    }

    #endregion

}
