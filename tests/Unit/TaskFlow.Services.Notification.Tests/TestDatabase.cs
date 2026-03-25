namespace TaskFlow.Services.Notification.Tests;

using Infrastructure;

using Microsoft.EntityFrameworkCore;

public static class TestDatabase
{

    #region Methods

    public static NotificationDbContext Create()
    {
        DbContextOptions<NotificationDbContext> options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new NotificationDbContext(options);
    }

    #endregion

}
