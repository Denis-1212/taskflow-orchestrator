namespace TaskFlow.Services.Task.Extensions;

using Middleware;

public static class RequestLoggingMiddlewareExtensions
{

    #region Methods

    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }

    #endregion

}
