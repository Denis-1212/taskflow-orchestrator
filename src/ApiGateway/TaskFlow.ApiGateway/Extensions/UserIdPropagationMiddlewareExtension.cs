namespace TaskFlow.ApiGateway.Extensions;

using Middleware;

public static class UserIdPropagationMiddlewareExtension
{

    #region Methods

    public static IApplicationBuilder UseUserIdPropagation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<UserIdPropagationMiddleware>();
    }

    #endregion

}
