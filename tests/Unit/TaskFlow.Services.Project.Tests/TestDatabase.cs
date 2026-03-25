namespace TaskFlow.Services.Project.Tests;

using Infrastructure;

using Microsoft.EntityFrameworkCore;

public static class TestDatabase
{

    #region Methods

    public static ProjectDbContext Create()
    {
        DbContextOptions<ProjectDbContext> options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ProjectDbContext(options);
    }

    #endregion

}
