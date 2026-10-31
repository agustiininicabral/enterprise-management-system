using EnterpriseManagement.Api.Domain;
using Microsoft.EntityFrameworkCore;
namespace EnterpriseManagement.Api.Data;
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<WorkTask> Tasks => Set<WorkTask>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e => {
            e.HasKey(x => x.Id); e.Property(x => x.Email).HasMaxLength(256).IsRequired();
            e.HasIndex(x => x.Email).IsUnique(); e.Property(x => x.PasswordHash).IsRequired();
            e.Property(x => x.Role).HasMaxLength(32).HasDefaultValue(User.DefaultRole).IsRequired();
        });
        modelBuilder.Entity<WorkTask>(e => {
            e.HasKey(x => x.Id); e.Property(x => x.Title).HasMaxLength(120).IsRequired();
            e.Property(x => x.Description).HasMaxLength(2000);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => new { x.UserId, x.CreatedAt });
            e.HasOne(x => x.User).WithMany(x => x.Tasks).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
