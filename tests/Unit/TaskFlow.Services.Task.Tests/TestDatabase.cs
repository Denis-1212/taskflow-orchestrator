namespace TaskFlow.Services.Task.Tests;

using Domain;

using Infrastructure;

using Microsoft.EntityFrameworkCore;

public static class TestDatabase
{

    #region Methods

    public static TaskDbContext Create()
    {
        DbContextOptions<TaskDbContext> options = new DbContextOptionsBuilder<TaskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(new DomainEventInterceptor())
            .Options;

        return new TaskDbContext(options);
    }

    #endregion

}
