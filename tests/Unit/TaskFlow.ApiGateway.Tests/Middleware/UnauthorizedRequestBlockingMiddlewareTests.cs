namespace TaskFlow.ApiGateway.Tests.Middleware;

using System.Security.Claims;

using ApiGateway.Middleware;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Moq;

public class UnauthorizedRequestBlockingMiddlewareTests
{

    #region Methods

    [Fact]
    public async Task InvokeAsync_WithPublicPath_ShouldAllowAccess()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UnauthorizedRequestBlockingMiddleware>>();
        bool nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new UnauthorizedRequestBlockingMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Request.Path = "/";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/login")]
    [InlineData("/register")]
    [InlineData("/auth/api/auth/register")]
    [InlineData("/auth/api/auth/login")]
    [InlineData("/auth/api/auth/refresh")]
    [InlineData("/health/live")]
    public async Task InvokeAsync_WithPublicPaths_ShouldAllowAccessWithoutAuth(string path)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UnauthorizedRequestBlockingMiddleware>>();
        bool nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new UnauthorizedRequestBlockingMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext
        {
            Request =
            {
                Path = path
            }
        };

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithProtectedPathAndNoAuth_ShouldBlockAccess()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UnauthorizedRequestBlockingMiddleware>>();
        bool nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new UnauthorizedRequestBlockingMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Request.Path = "/project/api/projects";

        // Ensure user is not authenticated
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        context.User = principal;

        // Act
        await middleware.InvokeAsync(context);

        // Assert - Verify that next middleware was NOT called (request was blocked)
        nextCalled.Should().BeFalse();
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unauthenticated request blocked")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithProtectedPathAndAuth_ShouldAllowAccess()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UnauthorizedRequestBlockingMiddleware>>();
        bool nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new UnauthorizedRequestBlockingMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext
        {
            Request =
            {
                Path = "/project/api/projects"
            }
        };

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user-123")
        };

        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);
        context.User = principal;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithCaseInsensitivePublicPath_ShouldAllowAccess()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UnauthorizedRequestBlockingMiddleware>>();
        bool nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new UnauthorizedRequestBlockingMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Request.Path = "/LOGIN";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_BlockedRequest_ShouldReturnAuthenticationRequiredMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UnauthorizedRequestBlockingMiddleware>>();
        bool nextCalled = false;

        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new UnauthorizedRequestBlockingMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Request.Path = "/task/api/tasks";

        // Ensure user is not authenticated
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        context.User = principal;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeFalse();
    }

    #endregion

}
