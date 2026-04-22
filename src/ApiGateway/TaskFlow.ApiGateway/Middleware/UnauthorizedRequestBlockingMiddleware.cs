namespace TaskFlow.ApiGateway.Middleware;

public class UnauthorizedRequestBlockingMiddleware(RequestDelegate next, ILogger<UnauthorizedRequestBlockingMiddleware> logger)
{

    #region Fields

    private readonly HashSet<string> _publicAuthPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/",
        "/login",
        "/register",
        "/auth/api/auth/register",
        "/auth/api/auth/login",
        "/auth/api/auth/refresh",
        "/health/live"
    };

    #endregion

    #region Methods

    public async Task InvokeAsync(HttpContext context)
    {
        bool isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;
        string path = context.Request.Path.Value ?? "";

        bool isPublicAuthRequest = _publicAuthPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));

        if (isPublicAuthRequest)
        {
            await next(context);
            return;
        }

        if (!isAuthenticated)
        {
            logger.LogWarning("Unauthenticated request blocked to {Path}", path);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Authentication required");
            return;
        }

        await next(context);
    }

    #endregion

}
