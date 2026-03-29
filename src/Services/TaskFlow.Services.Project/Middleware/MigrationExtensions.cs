namespace TaskFlow.Services.Project.Middleware;

public static class MigrationMiddlewareExtensions
{

    #region Methods

    public static IApplicationBuilder UseMigrations(this IApplicationBuilder app)
    {
        return app.UseMiddleware<MigrationMiddleware>();
    }

    #endregion

}
