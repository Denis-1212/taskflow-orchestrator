namespace TaskFlow.Services.Auth.Controllers;

using System.Security.Claims;

using Application.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Shared.DTOs;
using Shared.Kernel;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{

    #region Methods

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(CreateUserDto request)
    {
        string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        Result<AuthResult> result = await authService.RegisterAsync(request.Email, request.Password, request.FullName, ipAddress);

        if (result.IsFailure)
        {
            Error error = result.Error!;
            return error.Type switch
            {
                ErrorType.Conflict => Conflict(error),
                _ => BadRequest(error)
            };
        }

        return Ok(
            new AuthResponseDto(
                result.Value.AccessToken,
                result.Value.RefreshToken,
                new UserDto(
                    result.Value.User.Id,
                    result.Value.User.Email,
                    result.Value.User.FullName,
                    result.Value.User.IsActive,
                    result.Value.User.Roles)));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto request)
    {
        string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        Result<AuthResult> result = await authService.LoginAsync(request.Email, request.Password, ipAddress);

        if (result.IsFailure)
        {
            return Unauthorized(result.Error);
        }

        return Ok(
            new AuthResponseDto(
                result.Value.AccessToken,
                result.Value.RefreshToken,
                new UserDto(
                    result.Value.User.Id,
                    result.Value.User.Email,
                    result.Value.User.FullName,
                    result.Value.User.IsActive,
                    result.Value.User.Roles)));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] string refreshToken)
    {
        string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        Result<AuthResult> result = await authService.RefreshTokenAsync(refreshToken, ipAddress);

        if (result.IsFailure)
        {
            return Unauthorized(result.Error);
        }

        return Ok(
            new AuthResponseDto(
                result.Value.AccessToken,
                result.Value.RefreshToken,
                new UserDto(
                    result.Value.User.Id,
                    result.Value.User.Email,
                    result.Value.User.FullName,
                    result.Value.User.IsActive,
                    result.Value.User.Roles)));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] string refreshToken)
    {
        Result result = await authService.LogoutAsync(refreshToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(
            new
            {
                message = "Logged out successfully"
            });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            return Unauthorized(Error.Unauthorized("Invalid token"));
        }

        Result<UserResult> result = await authService.GetCurrentUserAsync(userId);

        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }

        return Ok(
            new UserDto(
                result.Value.Id,
                result.Value.Email,
                result.Value.FullName,
                result.Value.IsActive,
                result.Value.Roles));
    }

    #endregion

}
