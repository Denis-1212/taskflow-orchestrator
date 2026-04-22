namespace TaskFlow.ApiGateway.Extensions;

using Middleware;

public static class GlobalExceptionMiddlewareExtension
{

    #region Methods

    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }

    #endregion

}
