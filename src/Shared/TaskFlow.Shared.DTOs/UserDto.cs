namespace TaskFlow.Shared.DTOs;

public record UserDto(Guid Id, string Email, string FullName, bool IsActive, string[] Roles);
public record CreateUserDto(string Email, string Password, string FullName);
public record LoginDto(string Email, string Password);
public record AuthResponseDto(string AccessToken, string RefreshToken, UserDto User);
