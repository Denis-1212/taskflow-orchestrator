namespace TaskFlow.ApiGateway.Tests.Middleware;

using System.Security.Claims;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using TaskFlow.ApiGateway.Middleware;

using Moq;

public class UserIdPropagationMiddlewareTests
{

    #region Methods

    [Fact]
    public async Task InvokeAsync_WithValidUserIdClaim_ShouldAddXUserIdHeader()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UserIdPropagationMiddleware>>();
        var nextCalled = false;
        string? capturedUserId = null;

        RequestDelegate next = context =>
        {
            nextCalled = true;
            capturedUserId = context.Request.Headers["X-User-Id"].ToString();
            return Task.CompletedTask;
        };

        var middleware = new UserIdPropagationMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();

        var userId = "550e8400-e29b-41d4-a716-446655440000";
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);
        context.User = principal;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        capturedUserId.Should().Be(userId);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("Propagating user ID") &&
                    v.ToString()!.Contains(userId)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithoutUserIdClaim_ShouldNotAddXUserIdHeader()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UserIdPropagationMiddleware>>();
        var nextCalled = false;
        var headerAdded = false;

        RequestDelegate next = context =>
        {
            nextCalled = true;
            headerAdded = context.Request.Headers.ContainsKey("X-User-Id");
            return Task.CompletedTask;
        };

        var middleware = new UserIdPropagationMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();

        var claims = new List<Claim> { new Claim(ClaimTypes.Name, "testuser") };
        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);
        context.User = principal;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        headerAdded.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_WithUnauthenticatedUser_ShouldNotAddXUserIdHeader()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UserIdPropagationMiddleware>>();
        var nextCalled = false;
        var headerAdded = false;

        RequestDelegate next = context =>
        {
            nextCalled = true;
            headerAdded = context.Request.Headers.ContainsKey("X-User-Id");
            return Task.CompletedTask;
        };

        var middleware = new UserIdPropagationMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        // User is not set, so no authentication

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        headerAdded.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_WithEmptyUserIdClaim_ShouldNotAddXUserIdHeader()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UserIdPropagationMiddleware>>();
        var nextCalled = false;
        var headerAdded = false;

        RequestDelegate next = context =>
        {
            nextCalled = true;
            headerAdded = context.Request.Headers.ContainsKey("X-User-Id");
            return Task.CompletedTask;
        };

        var middleware = new UserIdPropagationMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();

        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "") };
        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);
        context.User = principal;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        headerAdded.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNextMiddleware()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UserIdPropagationMiddleware>>();
        var nextCalled = false;

        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new UserIdPropagationMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithMultipleUserIdClaims_ShouldUseFirstOne()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UserIdPropagationMiddleware>>();
        var capturedUserId = string.Empty;

        RequestDelegate next = context =>
        {
            capturedUserId = context.Request.Headers["X-User-Id"].ToString();
            return Task.CompletedTask;
        };

        var middleware = new UserIdPropagationMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();

        var userId = "550e8400-e29b-41d4-a716-446655440000";
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.NameIdentifier, "other-user-id")
        };
        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);
        context.User = principal;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        capturedUserId.Should().Be(userId);
    }

    #endregion

}
