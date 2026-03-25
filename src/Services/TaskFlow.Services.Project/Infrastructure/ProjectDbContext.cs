namespace TaskFlow.Services.Project.Infrastructure;

using Domain;

using Microsoft.EntityFrameworkCore;

public class ProjectDbContext(DbContextOptions<ProjectDbContext> options) : DbContext(options)
{

    #region Properties

    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectMember> ProjectMembers { get; set; }

    #endregion

    #region Methods

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.HasIndex(e => e.OwnerId);
            entity.HasIndex(e => e.IsDeleted);

            entity.HasMany(e => e.Members)
                .WithOne()
                .HasForeignKey(m => m.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.HasKey(e => new
            {
                e.ProjectId,
                e.UserId
            });

            entity.Property(e => e.Role).HasConversion<string>();
            entity.HasIndex(e => e.UserId);
        });
    }

    #endregion

}
