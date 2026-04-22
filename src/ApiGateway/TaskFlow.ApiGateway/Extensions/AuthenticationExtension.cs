namespace TaskFlow.ApiGateway.Extensions;

using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;

public static class AuthenticationExtensions
{

    #region Methods

    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        string? jwtSecret = configuration["Jwt:Secret"];

        if (string.IsNullOrEmpty(jwtSecret))
        {
            throw new InvalidOperationException("JWT Secret is not configured");
        }

        byte[] key = Encoding.UTF8.GetBytes(jwtSecret);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"] ?? "TaskFlow",
                    ValidAudience = configuration["Jwt:Audience"] ?? "TaskFlow",
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        string? token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                        if (string.IsNullOrEmpty(token) && context.Request.Query.TryGetValue("access_token", out StringValues queryToken))
                        {
                            token = queryToken;
                        }

                        if (!string.IsNullOrEmpty(token))
                        {
                            context.Token = token;
                        }

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            context.Response.Headers.Append("Token-Expired", "true");
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        }
                        else
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        }

                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }

    #endregion

}
