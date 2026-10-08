using LojaApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LojaApi.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).HasMaxLength(80).IsRequired();
            entity.HasIndex(c => c.Name).IsUnique();

            entity.HasData(
                new Category { Id = 1, Name = "Hardware" },
                new Category { Id = 2, Name = "Books" },
                new Category { Id = 3, Name = "Office" });
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).HasMaxLength(120).IsRequired();
            entity.Property(p => p.Price).HasPrecision(10, 2);
            entity.HasIndex(p => new { p.CategoryId, p.Name });

            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.HasIndex(m => new { m.ProductId, m.CreatedAt });

            entity.HasOne(m => m.Product)
                .WithMany(p => p.StockMovements)
                .HasForeignKey(m => m.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
