namespace TaskFlow.Services.Auth.Tests;

using Infrastructure;

using Microsoft.EntityFrameworkCore;

public static class TestDatabase
{

    #region Methods

    public static AuthDbContext Create()
    {
        DbContextOptions<AuthDbContext> options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AuthDbContext(options);
    }

    #endregion

}
