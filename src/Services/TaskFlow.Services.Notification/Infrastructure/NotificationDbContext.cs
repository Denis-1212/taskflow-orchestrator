namespace TaskFlow.Services.Notification.Infrastructure;

using Domain;

using Microsoft.EntityFrameworkCore;

public class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{

    #region Properties

    public DbSet<Notification> Notifications { get; set; }

    #endregion

    #region Methods

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Type).HasConversion<string>().IsRequired().HasMaxLength(50);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Metadata).HasMaxLength(1000);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.IsRead);
            entity.HasIndex(e => e.CreatedAt);
        });
    }

    #endregion

}
