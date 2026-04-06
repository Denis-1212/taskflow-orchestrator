namespace TaskFlow.Services.Project.Extensions;

using Middleware;

public static class UserIdMiddlewareExtensions
{

    #region Methods

    public static IApplicationBuilder UseUserIdExtraction(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<UserIdMiddleware>();
    }

    #endregion

}
