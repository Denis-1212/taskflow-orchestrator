namespace TaskFlow.Services.Notification.Extensions;

using Infrastructure;

using Microsoft.EntityFrameworkCore;

public static class MigrationMiddlewareExtensions
{

    #region Methods

    public static IApplicationBuilder UseMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        dbContext.Database.Migrate();
        return app;
    }

    #endregion

}
