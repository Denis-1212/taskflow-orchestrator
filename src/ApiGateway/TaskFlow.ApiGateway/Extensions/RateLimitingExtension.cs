namespace TaskFlow.ApiGateway.Extensions;

using System.Threading.RateLimiting;

using Microsoft.AspNetCore.RateLimiting;

public static class RateLimitingExtension
{

    #region Methods

    public static IServiceCollection AddCustomRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        RateLimitConfiguration rateLimitConfig =
            configuration.GetSection("RateLimiting").Get<RateLimitConfiguration>() ?? new RateLimitConfiguration();

        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                if (httpContext.User.Identity?.IsAuthenticated == true && !string.IsNullOrEmpty(httpContext.User.Identity.Name))
                {
                    return RateLimitPartition.GetFixedWindowLimiter(
                        httpContext.User.Identity.Name,
                        _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = rateLimitConfig.AuthenticatedPermitLimit,
                            QueueLimit = rateLimitConfig.QueueLimit,
                            Window = TimeSpan.FromMinutes(rateLimitConfig.WindowMinutes)
                        });
                }

                string key = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ??
                             httpContext.Connection.RemoteIpAddress?.ToString() ??
                             "anonymous";

                return RateLimitPartition.GetFixedWindowLimiter(
                    key,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = rateLimitConfig.UnauthenticatedPermitLimit,
                        QueueLimit = rateLimitConfig.QueueLimit,
                        Window = TimeSpan.FromMinutes(rateLimitConfig.WindowMinutes)
                    });
            });

            options.AddPolicy(
                "Auth",
                httpContext =>
                {
                    string key = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    return RateLimitPartition.GetFixedWindowLimiter(
                        key,
                        _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = rateLimitConfig.AuthPermitLimit,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        });
                });

            options.AddPolicy(
                "Strict",
                httpContext =>
                {
                    string key = httpContext.User.Identity?.Name ??
                                 httpContext.Connection.RemoteIpAddress?.ToString() ??
                                 "anonymous";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        key,
                        _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = rateLimitConfig.StrictPermitLimit,
                            QueueLimit = 0,
                            Window = TimeSpan.FromSeconds(rateLimitConfig.StrictWindowSeconds)
                        });
                });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<RateLimiterOptions>>();

                logger.LogWarning(
                    "Rate limit exceeded for {Client} on {Path}",
                    context.HttpContext.Connection.RemoteIpAddress,
                    context.HttpContext.Request.Path);

                context.HttpContext.Response.ContentType = "application/json";

                var problemDetails = new
                {
                    StatusCode = 429,
                    Message = rateLimitConfig.RejectionMessage ?? "Too many requests. Please try again later.",
                    RetryAfter = "60 seconds"
                };

                await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            };
        });

        return services;
    }

    #endregion

}

#region Models

public class RateLimitConfiguration
{

    #region Properties

    public int AuthenticatedPermitLimit { get; set; } = 100;
    public int UnauthenticatedPermitLimit { get; set; } = 10;
    public int AuthPermitLimit { get; set; } = 5;
    public int StrictPermitLimit { get; set; } = 3;
    public int WindowMinutes { get; set; } = 1;
    public int StrictWindowSeconds { get; set; } = 10;
    public int QueueLimit { get; set; }
    public string RejectionMessage { get; set; } = "Too many requests. Please try again later.";

    #endregion

}

#endregion
