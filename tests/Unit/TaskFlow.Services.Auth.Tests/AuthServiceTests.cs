namespace TaskFlow.Services.Auth.Tests;

using System.Reflection;

using Application.Services;

using Domain;

using FluentAssertions;

using Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

using Shared.Kernel;

public class AuthServiceTests : IDisposable
{

    #region Fields

    private readonly AuthDbContext _context;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenService> _jwtServiceMock;
    private readonly AuthService _authService;

    #endregion

    #region Constructors

    public AuthServiceTests()
    {
        _context = TestDatabase.Create();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtServiceMock = new Mock<IJwtTokenService>();
        var loggerMock = new Mock<ILogger<AuthService>>();

        _authService = new AuthService(
            _context,
            _passwordHasherMock.Object,
            _jwtServiceMock.Object,
            loggerMock.Object);
    }

    #endregion

    #region Methods

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task RegisterAsync_WithNewUser_ShouldSucceed()
    {
        // Arrange
        string email = "test@example.com";
        string password = "Password123!";
        string fullName = "Test User";
        string ipAddress = "127.0.0.1";
        string passwordHash = "hashed_password";
        string accessToken = "access_token";
        string expectedRefreshToken = "expected_refresh_token";

        _passwordHasherMock.Setup(x => x.Hash(password)).Returns(passwordHash);
        _jwtServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<User>())).Returns(accessToken);

        // Создаем mock refresh token с нужным значением
        var mockRefreshToken = new RefreshToken(Guid.NewGuid(), ipAddress, TimeSpan.FromDays(7));
        FieldInfo? tokenField = typeof(RefreshToken).GetField(
            "<Token>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic);

        tokenField?.SetValue(mockRefreshToken, expectedRefreshToken);

        _jwtServiceMock.Setup(x => x.GenerateRefreshToken(It.IsAny<Guid>(), ipAddress))
            .Returns(mockRefreshToken);

        // Act
        Result<AuthResult> result = await _authService.RegisterAsync(email, password, fullName, ipAddress);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.AccessToken.Should().Be(accessToken);
        result.Value.RefreshToken.Should().Be(expectedRefreshToken);
        result.Value.User.Email.Should().Be(email);
        result.Value.User.FullName.Should().Be(fullName);

        User? userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        userInDb.Should().NotBeNull();
        userInDb!.Email.Should().Be(email);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldReturnConflict()
    {
        // Arrange
        string email = "existing@example.com";
        string password = "Password123!";
        string fullName = "Existing User";
        string ipAddress = "127.0.0.1";
        string passwordHash = "hashed_password";

        _passwordHasherMock.Setup(x => x.Hash(password)).Returns(passwordHash);

        // Create existing user
        var existingUser = new User(email, passwordHash, fullName);
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        // Act
        Result<AuthResult> result = await _authService.RegisterAsync(email, password, fullName, ipAddress);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
        result.Error.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldSucceed()
    {
        // Arrange
        string email = "user@example.com";
        string password = "Password123!";
        string ipAddress = "127.0.0.1";
        string passwordHash = "hashed_password";
        string accessToken = "access_token";

        _passwordHasherMock.Setup(x => x.Verify(passwordHash, password)).Returns(true);
        _jwtServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<User>())).Returns(accessToken);
        _jwtServiceMock.Setup(x => x.GenerateRefreshToken(It.IsAny<Guid>(), ipAddress))
            .Returns(new RefreshToken(Guid.NewGuid(), ipAddress, TimeSpan.FromDays(7)));

        // Create user
        var user = new User(email, passwordHash, "Test User");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        Result<AuthResult> result = await _authService.LoginAsync(email, password, ipAddress);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be(accessToken);
        result.Value.User.Email.Should().Be(email);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        string email = "user@example.com";
        string password = "WrongPassword";
        string ipAddress = "127.0.0.1";
        string passwordHash = "hashed_password";

        _passwordHasherMock.Setup(x => x.Verify(passwordHash, password)).Returns(false);

        var user = new User(email, passwordHash, "Test User");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        Result<AuthResult> result = await _authService.LoginAsync(email, password, ipAddress);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error?.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ShouldReturnUnauthorized()
    {
        // Arrange
        string email = "nonexistent@example.com";
        string password = "Password123!";
        string ipAddress = "127.0.0.1";

        // Act
        Result<AuthResult> result = await _authService.LoginAsync(email, password, ipAddress);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error?.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ShouldReturnNewTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        string ipAddress = "127.0.0.1";
        string refreshTokenValue = "valid_refresh_token";
        string newAccessToken = "new_access_token";

        var user = new User("user@example.com", "hash", "Test User");
        PropertyInfo? userProperty = user.GetType().GetProperty("Id");
        userProperty?.SetValue(user, userId);

        var refreshToken = new RefreshToken(userId, "old_ip", TimeSpan.FromDays(7));
        PropertyInfo? refreshTokenProperty = refreshToken.GetType().GetProperty("Token");
        refreshTokenProperty?.SetValue(refreshToken, refreshTokenValue);

        _context.Users.Add(user);
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        _jwtServiceMock.Setup(x => x.GenerateAccessToken(user)).Returns(newAccessToken);
        _jwtServiceMock.Setup(x => x.GenerateRefreshToken(userId, ipAddress))
            .Returns(new RefreshToken(userId, ipAddress, TimeSpan.FromDays(7)));

        // Act
        Result<AuthResult> result = await _authService.RefreshTokenAsync(refreshTokenValue, ipAddress);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be(newAccessToken);

        RefreshToken? oldToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshTokenValue);
        oldToken.Should().NotBeNull();
        oldToken!.IsValid().Should().BeFalse();
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        string refreshTokenValue = "expired_token";

        var refreshToken = new RefreshToken(userId, "ip", TimeSpan.FromDays(-1));
        PropertyInfo? tokenProperty = refreshToken.GetType().GetProperty("Token");
        tokenProperty?.SetValue(refreshToken, refreshTokenValue);

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        // Act
        Result<AuthResult> result = await _authService.RefreshTokenAsync(refreshTokenValue, "127.0.0.1");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error?.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task LogoutAsync_WithValidToken_ShouldRevokeToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        string refreshTokenValue = "valid_token";

        var refreshToken = new RefreshToken(userId, "ip", TimeSpan.FromDays(7));
        PropertyInfo? tokenProperty = refreshToken.GetType().GetProperty("Token");
        tokenProperty?.SetValue(refreshToken, refreshTokenValue);

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        // Act
        Result result = await _authService.LogoutAsync(refreshTokenValue);

        // Assert
        result.IsSuccess.Should().BeTrue();

        RefreshToken? token = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshTokenValue);
        token.Should().NotBeNull();
        token!.IsValid().Should().BeFalse();
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithExistingUser_ShouldReturnUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        string email = "user@example.com";
        string fullName = "Test User";

        var user = new User(email, "hash", fullName);
        PropertyInfo? userProperty = user.GetType().GetProperty("Id");
        userProperty?.SetValue(user, userId);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        Result<UserResult> result = await _authService.GetCurrentUserAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(userId);
        result.Value.Email.Should().Be(email);
        result.Value.FullName.Should().Be(fullName);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithNonExistentUser_ShouldReturnNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        Result<UserResult> result = await _authService.GetCurrentUserAsync(userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error?.Type.Should().Be(ErrorType.NotFound);
    }

    #endregion

}
