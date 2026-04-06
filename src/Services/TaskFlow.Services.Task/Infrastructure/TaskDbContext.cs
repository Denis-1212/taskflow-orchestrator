namespace TaskFlow.Services.Task.Infrastructure;

using Domain;

using Microsoft.EntityFrameworkCore;

public class TaskDbContext(DbContextOptions<TaskDbContext> options) : DbContext(options)
{

    #region Properties

    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<TaskStatusHistory> TaskStatusHistories { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    #endregion

    #region Methods

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.Priority).HasConversion<int>();
            entity.HasIndex(e => e.ProjectId);
            entity.HasIndex(e => e.AssigneeId);
            entity.HasIndex(e => e.IsDeleted);

            // entity.HasMany(e => e.StatusHistory)
            //     .WithOne()
            //     .HasForeignKey(h => h.TaskId)
            //     .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TaskStatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OldStatus).HasConversion<int>();
            entity.Property(e => e.NewStatus).HasConversion<int>();
            entity.Property(e => e.Comment).HasMaxLength(500);
            entity.HasIndex(e => e.TaskId);
            entity.HasIndex(e => e.ChangedAt);
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(200);
            entity.Property(e => e.EventData).IsRequired();
            entity.Property(e => e.EventData);
            entity.HasIndex(e => e.ProcessedAt);
            entity.HasIndex(e => e.CreatedAt);
        });
    }

    #endregion

}
