namespace TaskFlow.ApiGateway.Tests.Middleware;

using System.Security.Claims;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using TaskFlow.ApiGateway.Middleware;

using Moq;

public class GlobalExceptionMiddlewareTests
{

    #region Methods

    [Fact]
    public async Task InvokeAsync_WithValidRequest_ShouldCallNextMiddleware()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new GlobalExceptionMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithException_ShouldLogErrorAndRethrow()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
        var testException = new InvalidOperationException("Test error");

        RequestDelegate next = _ => throw testException;

        var middleware = new GlobalExceptionMiddleware(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/test-path";

        // Act
        Func<Task> act = async () => await middleware.InvokeAsync(context);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("Unhandled exception occurred") &&
                    v.ToString()!.Contains("GET") &&
                    v.ToString()!.Contains("/test-path")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithMultipleRequests_ShouldHandleEachIndependently()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
        var callCount = 0;
        RequestDelegate next = _ =>
        {
            callCount++;
            return Task.CompletedTask;
        };

        var middleware = new GlobalExceptionMiddleware(next, loggerMock.Object);

        // Act
        for (int i = 0; i < 3; i++)
        {
            var context = new DefaultHttpContext();
            await middleware.InvokeAsync(context);
        }

        // Assert
        callCount.Should().Be(3);
    }

    #endregion

}
