namespace TaskFlow.Services.Auth.Extensions;

using Infrastructure;

using Microsoft.EntityFrameworkCore;

public static class MigrationMiddlewareExtensions
{

    #region Methods

    public static IApplicationBuilder UseMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        dbContext.Database.Migrate();
        return app;
    }

    #endregion

}
