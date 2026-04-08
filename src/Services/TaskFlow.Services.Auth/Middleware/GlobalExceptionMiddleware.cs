namespace TaskFlow.Services.Auth.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{

    #region Methods

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unhandled exception occurred processing {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            throw;
        }
    }

    #endregion

}
