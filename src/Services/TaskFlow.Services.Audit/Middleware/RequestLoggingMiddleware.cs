namespace TaskFlow.Services.Audit.Middleware;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{

    #region Methods

    public async Task InvokeAsync(HttpContext context)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
        }

        await next(context);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Response: {StatusCode} {Method} {Path}",
                context.Response.StatusCode,
                context.Request.Method,
                context.Request.Path);
        }
    }

    #endregion

}
