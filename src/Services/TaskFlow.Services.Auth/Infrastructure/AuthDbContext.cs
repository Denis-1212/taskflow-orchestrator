namespace TaskFlow.Services.Auth.Infrastructure;

using Domain;

using Microsoft.EntityFrameworkCore;

public class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{

    #region Properties

    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    #endregion

    #region Methods

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Roles).HasColumnType("jsonb");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Token);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(256);
            entity.HasIndex(e => e.UserId);
        });
    }

    #endregion

}
