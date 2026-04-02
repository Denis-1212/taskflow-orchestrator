namespace TaskFlow.Services.Auth.Application.Services;

using Shared.Kernel;

public interface IAuthService
{

    #region Methods

    Task<Result<AuthResult>> RegisterAsync(string email, string password, string fullName, string ipAddress);
    Task<Result<AuthResult>> LoginAsync(string email, string password, string ipAddress);
    Task<Result<AuthResult>> RefreshTokenAsync(string refreshToken, string ipAddress);
    Task<Result> LogoutAsync(string refreshToken);
    Task<Result<UserResult>> GetCurrentUserAsync(Guid userId);
    Task<Result<UserResult>> GetUserByIdAsync(Guid userId);
    Task<Result<UserResult>> GetUserByEmailAsync(string email);
    Task<bool> ValidateToken(string requestToken);
    Task<Result<string[]>> GetUserRoles(Guid userId);
    Task<Result<IEnumerable<UserResult>>> GetUsersAsync(string query);

    #endregion

}

public record AuthResult(string AccessToken, string RefreshToken, UserResult User);

public record UserResult(Guid Id, string Email, string FullName, bool IsActive, string[] Roles);
