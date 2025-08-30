using Microsoft.EntityFrameworkCore;
using RunEF.WebServer.Domain.Entities;

namespace RunEF.WebServer.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // public DbSet<User> Users { get; set; } // Tạm thời comment out vì đã có DataRunEFAccountWeb
    public DbSet<RunEFClient> RunEFClients { get; set; }
    public DbSet<ApplicationLog> ApplicationLogs { get; set; }
    public DbSet<DataRunEFAccountWeb> DataRunEFAccountWebs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User entity configuration - tạm thời comment out
        /*
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role)
                  .HasDefaultValue("User");
            entity.HasIndex(e => e.Username).IsUnique();
        });
        */

        // RunEFClient entity configuration - sử dụng Table attribute thay vì cấu hình ở đây
        modelBuilder.Entity<RunEFClient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ComputerCode).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.HasIndex(e => e.ComputerCode).IsUnique();
        });

        // ApplicationLog entity configuration - sử dụng Table attribute thay vì cấu hình ở đây
        modelBuilder.Entity<ApplicationLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).HasMaxLength(100);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.ComputerCode).HasMaxLength(100);
        });

        // DataRunEFAccountWeb entity configuration
        modelBuilder.Entity<DataRunEFAccountWeb>(entity =>
        {
            entity.ToTable("datarunefaccountweb");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role)
                  .HasDefaultValue("User");
            entity.HasIndex(e => e.Username).IsUnique();
        });
    }
}