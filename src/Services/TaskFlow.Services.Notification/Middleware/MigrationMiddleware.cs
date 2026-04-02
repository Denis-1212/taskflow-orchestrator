namespace TaskFlow.Services.Notification.Middleware;

using Infrastructure;

using Microsoft.EntityFrameworkCore;

public class MigrationMiddleware(RequestDelegate next)
{

    #region Methods

    public async Task InvokeAsync(HttpContext context, NotificationDbContext dbContext, ILogger<MigrationMiddleware> logger)
    {
        try
        {
            logger.LogInformation("Applying database migrations...");
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Migrations applied successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to apply migrations");
            throw;
        }

        await next(context);
    }

    #endregion

}
