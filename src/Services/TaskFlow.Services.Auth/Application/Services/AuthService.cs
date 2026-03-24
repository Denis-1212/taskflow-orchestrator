namespace TaskFlow.Services.Auth.Application.Services;

using Domain;

using Infrastructure;

using Microsoft.EntityFrameworkCore;

using Shared.Kernel;

public class AuthService(
    AuthDbContext context,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtService,
    ILogger<AuthService> logger)
    : IAuthService
{

    #region Methods

    public async Task<Result<AuthResult>> RegisterAsync(string email, string password, string fullName, string ipAddress)
    {
        logger.LogInformation("Register attempt for email: {Email}", email);

        User? existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (existingUser != null)
        {
            return Error.Conflict("User with this email already exists");
        }

        string passwordHash = passwordHasher.Hash(password);
        var user = new User(email, passwordHash, fullName);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        string accessToken = jwtService.GenerateAccessToken(user);
        RefreshToken refreshToken = jwtService.GenerateRefreshToken(user.Id, ipAddress);

        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync();

        logger.LogInformation("User registered successfully: {UserId}", user.Id);

        return new AuthResult(
            accessToken,
            refreshToken.Token,
            new UserResult(user.Id, user.Email, user.FullName, user.IsActive, user.Roles.ToArray()));
    }

    public async Task<Result<AuthResult>> LoginAsync(string email, string password, string ipAddress)
    {
        logger.LogInformation("Login attempt for email: {Email}", email);

        User? user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            return Error.Unauthorized("Invalid email or password");
        }

        if (!user.IsActive)
        {
            return Error.Unauthorized("Account is deactivated");
        }

        if (!user.VerifyPassword(password, passwordHasher))
        {
            return Error.Unauthorized("Invalid email or password");
        }

        string accessToken = jwtService.GenerateAccessToken(user);
        RefreshToken refreshToken = jwtService.GenerateRefreshToken(user.Id, ipAddress);

        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync();

        logger.LogInformation("User logged in: {UserId}", user.Id);

        return new AuthResult(
            accessToken,
            refreshToken.Token,
            new UserResult(user.Id, user.Email, user.FullName, user.IsActive, user.Roles.ToArray()));
    }

    public async Task<Result<AuthResult>> RefreshTokenAsync(string refreshToken, string ipAddress)
    {
        logger.LogInformation("Refresh token attempt");

        RefreshToken? token = await context.RefreshTokens
                                  .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (token == null || !token.IsValid())
        {
            return Error.Unauthorized("Invalid or expired refresh token");
        }

        User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == token.UserId);

        if (user == null || !user.IsActive)
        {
            return Error.Unauthorized("User not found or inactive");
        }

        token.Revoke();

        string newAccessToken = jwtService.GenerateAccessToken(user);
        RefreshToken newRefreshToken = jwtService.GenerateRefreshToken(user.Id, ipAddress);

        context.RefreshTokens.Add(newRefreshToken);
        await context.SaveChangesAsync();

        logger.LogInformation("Token refreshed for user: {UserId}", user.Id);

        return new AuthResult(
            newAccessToken,
            newRefreshToken.Token,
            new UserResult(user.Id, user.Email, user.FullName, user.IsActive, user.Roles.ToArray()));
    }

    public async Task<Result> LogoutAsync(string refreshToken)
    {
        logger.LogInformation("Logout attempt");

        RefreshToken? token = await context.RefreshTokens
                                  .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (token != null)
        {
            token.Revoke();
            await context.SaveChangesAsync();
        }

        logger.LogInformation("User logged out");
        return Result.Success();
    }

    public async Task<Result<UserResult>> GetCurrentUserAsync(Guid userId)
    {
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return Error.NotFound("User", userId);
        }

        return new UserResult(user.Id, user.Email, user.FullName, user.IsActive, user.Roles.ToArray());
    }

    #endregion

}
