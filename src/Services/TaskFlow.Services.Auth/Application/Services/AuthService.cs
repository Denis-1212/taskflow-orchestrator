namespace TaskFlow.Services.Auth.Application.Services;

using Auth.Services;

using Domain;

using Infrastructure;

using Microsoft.EntityFrameworkCore;

using RabbitMQ.Module.Contracts;

using Shared.Kernel;
using Shared.Messaging.Events;

public class AuthService(
    AuthDbContext context,
    IJwtService jwtService,
    IRefreshTokenService refreshTokenService,
    IPasswordHasher passwordHasher,
    IPublisher publisher,
    ILogger<AuthService> logger)
    : IAuthService
{

    #region Constants

    private const string USER_REGISTERED_ROUTING_KEY = "user.registered";
    private const string EXCHANGE_NAME = "taskflow.events";

    #endregion

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

        User? existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (existingUser != null)
        {
            return Error.Conflict($"User with email {email} already exists");
        }

        string passwordHash = passwordHasher.Hash(password);

        var user = new User(email, passwordHash, fullName);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        string accessToken = jwtService.GenerateAccessToken(user);
        string refreshToken = jwtService.GenerateRefreshToken();

        await refreshTokenService.SaveRefreshTokenAsync(
            refreshToken,
            user.Id,
            ipAddress,
            TimeSpan.FromDays(7));

        var userRegisteredEvent = new UserRegisteredEvent
        {
            UserId = user.Id,
            Email = email,
            FullName = fullName
        };

        await publisher.PublishAsync(
            userRegisteredEvent,
            c =>
            {
                c.WithExchange(EXCHANGE_NAME);
                c.WithRoutingKey(USER_REGISTERED_ROUTING_KEY);
            });

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

        await refreshTokenService.RemoveRefreshTokenAsync(refreshToken);

        string newAccessToken = jwtService.GenerateAccessToken(user);
        string newRefreshToken = jwtService.GenerateRefreshToken();

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

    public async Task<bool> ValidateToken(string requestToken)
    {
        RefreshTokenData? tokenData = await refreshTokenService.GetRefreshTokenAsync(requestToken);

        return tokenData != null;
    }

    public async Task<Result<string[]>> GetUserRoles(Guid userId)
    {
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return Error.NotFound("User", userId.ToString());
        }

        return user.Roles.ToArray();
    }

    public async Task<Result<IEnumerable<UserResult>>> GetUsersAsync(string query)
    {
        try
        {
            IQueryable<User> users = context.Users.Where(user => user.Email.Contains(query) || user.FullName.Contains(query));

            var userResults = new List<UserResult>();

            await users.ForEachAsync(user => userResults.Add(
                new UserResult(user.Id, user.Email, user.FullName, user.IsActive, user.Roles.ToArray())));

            return await Task.FromResult<Result<IEnumerable<UserResult>>>(userResults);
        }
        catch (Exception exception)
        {
            return await Task.FromException<Result<IEnumerable<UserResult>>>(exception);
        }
    }

    #endregion

}
