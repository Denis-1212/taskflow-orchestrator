namespace TaskFlow.Services.Notification.Extensions;

using Middleware;

public static class GlobalExceptionMiddlewareExtensions
{

    #region Methods

    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }

    #endregion

}
