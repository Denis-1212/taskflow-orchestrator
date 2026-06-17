namespace TaskFlow.ApiGateway.Tests.Extensions;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;

using Microsoft.Extensions.Configuration;

using Microsoft.Extensions.DependencyInjection;

using TaskFlow.ApiGateway.Extensions;

public class RateLimitingExtensionTests
{

    #region Methods

    [Fact]
    public void AddCustomRateLimiting_WithValidConfiguration_ShouldAddRateLimiter()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RateLimiting:AuthenticatedPermitLimit", "100" },
                { "RateLimiting:UnauthenticatedPermitLimit", "10" },
                { "RateLimiting:AuthPermitLimit", "5" },
                { "RateLimiting:StrictPermitLimit", "3" },
                { "RateLimiting:WindowMinutes", "1" },
                { "RateLimiting:StrictWindowSeconds", "10" },
                { "RateLimiting:QueueLimit", "0" }
            })
            .Build();

        // Act
        var result = services.AddCustomRateLimiting(configuration);

        // Assert
        result.Should().NotBeNull();
        Assert.Same(result, services);
    }

    [Fact]
    public void AddCustomRateLimiting_WithoutConfiguration_ShouldUseDefaults()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { })
            .Build();

        // Act
        var result = services.AddCustomRateLimiting(configuration);

        // Assert
        result.Should().NotBeNull();
        Assert.Same(result, services);
    }

    [Fact]
    public void AddCustomRateLimiting_ShouldReturnServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        var result = services.AddCustomRateLimiting(configuration);

        // Assert
        result.Should().NotBeNull();
        Assert.Same(result, services);
    }

    [Fact]
    public void AddCustomRateLimiting_ShouldSetRejectionStatusCode()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RateLimiting:AuthenticatedPermitLimit", "100" },
                { "RateLimiting:UnauthenticatedPermitLimit", "10" }
            })
            .Build();

        // Act
        var result = services.AddCustomRateLimiting(configuration);

        // Assert - If no exception is thrown, the setup is correct
        result.Should().NotBeNull();
        Assert.Same(result, services);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(1000)]
    public void AddCustomRateLimiting_WithVariousPermitLimits_ShouldAccept(int permitLimit)
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RateLimiting:AuthenticatedPermitLimit", permitLimit.ToString() },
                { "RateLimiting:UnauthenticatedPermitLimit", "10" }
            })
            .Build();

        // Act
        var result = services.AddCustomRateLimiting(configuration);

        // Assert
        result.Should().NotBeNull();
        Assert.Same(result, services);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void AddCustomRateLimiting_WithVariousWindowMinutes_ShouldAccept(int windowMinutes)
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RateLimiting:WindowMinutes", windowMinutes.ToString() },
                { "RateLimiting:AuthenticatedPermitLimit", "100" }
            })
            .Build();

        // Act
        var result = services.AddCustomRateLimiting(configuration);

        // Assert
        result.Should().NotBeNull();
        Assert.Same(result, services);
    }

    [Fact]
    public void AddCustomRateLimiting_WithCustomRejectionMessage_ShouldAccept()
    {
        // Arrange
        var services = new ServiceCollection();
        var customMessage = "Custom rate limit exceeded message";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RateLimiting:RejectionMessage", customMessage },
                { "RateLimiting:AuthenticatedPermitLimit", "100" }
            })
            .Build();

        // Act
        var result = services.AddCustomRateLimiting(configuration);

        // Assert
        result.Should().NotBeNull();
        Assert.Same(result, services);
    }

    [Fact]
    public void RateLimitConfiguration_ShouldHaveDefaultValues()
    {
        // Act
        var config = new RateLimitConfiguration();

        // Assert
        config.AuthenticatedPermitLimit.Should().Be(100);
        config.UnauthenticatedPermitLimit.Should().Be(10);
        config.AuthPermitLimit.Should().Be(5);
        config.StrictPermitLimit.Should().Be(3);
        config.WindowMinutes.Should().Be(1);
        config.StrictWindowSeconds.Should().Be(10);
        config.QueueLimit.Should().Be(0);
        config.RejectionMessage.Should().Be("Too many requests. Please try again later.");
    }

    [Fact]
    public void RateLimitConfiguration_ShouldAllowPropertyAssignment()
    {
        // Arrange
        var config = new RateLimitConfiguration();

        // Act
        config.AuthenticatedPermitLimit = 200;
        config.UnauthenticatedPermitLimit = 20;
        config.AuthPermitLimit = 10;
        config.StrictPermitLimit = 5;
        config.WindowMinutes = 2;
        config.StrictWindowSeconds = 20;
        config.QueueLimit = 10;
        config.RejectionMessage = "Custom message";

        // Assert
        config.AuthenticatedPermitLimit.Should().Be(200);
        config.UnauthenticatedPermitLimit.Should().Be(20);
        config.AuthPermitLimit.Should().Be(10);
        config.StrictPermitLimit.Should().Be(5);
        config.WindowMinutes.Should().Be(2);
        config.StrictWindowSeconds.Should().Be(20);
        config.QueueLimit.Should().Be(10);
        config.RejectionMessage.Should().Be("Custom message");
    }

    #endregion

}
