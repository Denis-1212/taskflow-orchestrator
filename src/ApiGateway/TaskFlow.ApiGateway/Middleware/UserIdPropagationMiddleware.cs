namespace TaskFlow.ApiGateway.Middleware;

using System.Security.Claims;

public class UserIdPropagationMiddleware(RequestDelegate next, ILogger<UserIdPropagationMiddleware> logger)
{

    #region Methods

    public async Task InvokeAsync(HttpContext context)
    {
        string? userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            context.Request.Headers.Append("X-User-Id", userId);
            logger.LogDebug("Propagating user ID: {UserId} to downstream service", userId);
        }

        await next(context);
    }

    #endregion

}
