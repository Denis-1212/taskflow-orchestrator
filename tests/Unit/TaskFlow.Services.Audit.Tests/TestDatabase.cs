namespace TaskFlow.Services.Audit.Tests;

using Infrastructure;

using Microsoft.EntityFrameworkCore;

public static class TestDatabase
{

    #region Methods

    public static AuditDbContext Create()
    {
        DbContextOptions<AuditDbContext> options = new DbContextOptionsBuilder<AuditDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AuditDbContext(options);
    }

    #endregion

}
