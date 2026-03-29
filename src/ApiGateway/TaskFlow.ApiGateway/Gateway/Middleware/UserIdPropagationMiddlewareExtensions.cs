namespace TaskFlow.ApiGateway.Gateway.Middleware;

public static class UserIdPropagationMiddlewareExtensions
{

    #region Methods

    public static IApplicationBuilder UseUserIdPropagation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<UserIdPropagationMiddleware>();
    }

    #endregion

}
