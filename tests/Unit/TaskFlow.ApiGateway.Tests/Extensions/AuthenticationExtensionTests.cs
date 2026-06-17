namespace TaskFlow.ApiGateway.Tests.Extensions;

using FluentAssertions;

using Microsoft.AspNetCore.Authentication;

using Microsoft.Extensions.Configuration;

using Microsoft.Extensions.DependencyInjection;

using TaskFlow.ApiGateway.Extensions;

public class AuthenticationExtensionTests
{

    #region Methods

    [Fact]
    public void AddCustomAuthentication_WithValidJwtSecret_ShouldReturnServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Secret", "my-super-secret-key-at-least-32-characters-long" },
                { "Jwt:Issuer", "TaskFlow" },
                { "Jwt:Audience", "TaskFlow" }
            })
            .Build();

        // Act
        var result = services.AddCustomAuthentication(configuration);

        // Assert
        result.Should().NotBeNull();
        Assert.Same(result, services);
    }

    [Fact]
    public void AddCustomAuthentication_WithoutJwtSecret_ShouldThrowException()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Issuer", "TaskFlow" },
                { "Jwt:Audience", "TaskFlow" }
            })
            .Build();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => services.AddCustomAuthentication(configuration));
    }

    [Fact]
    public void AddCustomAuthentication_WithEmptyJwtSecret_ShouldThrowException()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Secret", "" },
                { "Jwt:Issuer", "TaskFlow" },
                { "Jwt:Audience", "TaskFlow" }
            })
            .Build();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => services.AddCustomAuthentication(configuration));
    }

    [Fact]
    public void AddCustomAuthentication_WithNullJwtSecret_ShouldThrowException()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Issuer", "TaskFlow" },
                { "Jwt:Audience", "TaskFlow" }
            })
            .Build();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => services.AddCustomAuthentication(configuration));
    }

    [Fact]
    public void AddCustomAuthentication_WithValidSecret_ShouldReturnServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Secret", "my-super-secret-key-at-least-32-characters-long" },
                { "Jwt:Issuer", "TaskFlow" },
                { "Jwt:Audience", "TaskFlow" }
            })
            .Build();

        // Act
        var result = services.AddCustomAuthentication(configuration);

        // Assert
        result.Should().NotBeNull();
        Assert.Same(result, services);
    }

    [Fact]
    public void AddCustomAuthentication_WithDefaultIssuerAndAudience_ShouldSucceed()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Secret", "my-super-secret-key-at-least-32-characters-long" }
            })
            .Build();

        // Act
        var result = services.AddCustomAuthentication(configuration);

        // Assert
        result.Should().NotBeNull();
        Assert.Same(result, services);
    }

    [Fact]
    public void AddCustomAuthentication_ShouldRegisterAuthenticationService()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Secret", "my-super-secret-key-at-least-32-characters-long" },
                { "Jwt:Issuer", "TaskFlow" },
                { "Jwt:Audience", "TaskFlow" }
            })
            .Build();

        // Act
        services.AddCustomAuthentication(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var authService = serviceProvider.GetService<IAuthenticationService>();
        authService.Should().NotBeNull();
    }

    [Fact]
    public void AddCustomAuthentication_WithLongSecret_ShouldAccept()
    {
        // Arrange
        var services = new ServiceCollection();
        var longSecret = new string('x', 256); // Very long secret
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Secret", longSecret },
                { "Jwt:Issuer", "CustomIssuer" },
                { "Jwt:Audience", "CustomAudience" }
            })
            .Build();

        // Act
        var result = services.AddCustomAuthentication(configuration);

        // Assert
        result.Should().NotBeNull();
        Assert.Same(result, services);
    }

    [Fact]
    public void AddCustomAuthentication_WithCustomIssuerAndAudience_ShouldAccept()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Secret", "my-super-secret-key-at-least-32-characters-long" },
                { "Jwt:Issuer", "CustomIssuer" },
                { "Jwt:Audience", "CustomAudience" }
            })
            .Build();

        // Act
        var result = services.AddCustomAuthentication(configuration);

        // Assert
        result.Should().NotBeNull();
        Assert.Same(result, services);
    }

    #endregion

}
