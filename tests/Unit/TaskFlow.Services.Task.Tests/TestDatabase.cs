namespace TaskFlow.Services.Task.Tests;

using Infrastructure;

using Microsoft.EntityFrameworkCore;

public static class TestDatabase
{

    #region Methods

    public static TaskDbContext Create()
    {
        DbContextOptions<TaskDbContext> options = new DbContextOptionsBuilder<TaskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TaskDbContext(options);
    }

    #endregion

}
