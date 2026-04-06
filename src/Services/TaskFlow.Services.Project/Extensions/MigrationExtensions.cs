namespace TaskFlow.Services.Project.Extensions;

using Infrastructure;

using Microsoft.EntityFrameworkCore;

public static class MigrationMiddlewareExtensions
{

    #region Methods

    public static IApplicationBuilder UseMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ProjectDbContext>();
        dbContext.Database.Migrate();
        return app;
    }

    #endregion

}
