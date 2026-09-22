using Heimevernet.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Heimevernet.Web.DataAccess;

public class HeimevernetDbContext(DbContextOptions<HeimevernetDbContext> options) : DbContext(options)
{
    public DbSet<Resource> Resources => Set<Resource>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Resource>(entity =>
        {
            entity.ToTable("Resources");
            entity.HasKey(resource => resource.Id);
            entity.Property(resource => resource.Name)
                .HasMaxLength(200)
                .IsRequired();
            entity.Property(resource => resource.Description)
                .HasMaxLength(2000)
                .IsRequired();
            entity.Property(resource => resource.Type)
                .HasMaxLength(100)
                .IsRequired();
        });
    }
}
