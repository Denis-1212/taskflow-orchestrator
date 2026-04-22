namespace TaskFlow.ApiGateway.Extensions;

using Middleware;

public static class UnauthorizedRequestBlockingExtension
{

    #region Methods

    public static IApplicationBuilder UnauthorizedRequestBlocking(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<UnauthorizedRequestBlockingMiddleware>();
    }

    #endregion

}
