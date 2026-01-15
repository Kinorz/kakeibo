using Kakeibo.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kakeibo.Core.Data;

public sealed class KakeiboDbContext : DbContext
{
    public KakeiboDbContext(DbContextOptions<KakeiboDbContext> options) : base(options)
    {
    }

    public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();
    public DbSet<TransactionEntity> Transactions => Set<TransactionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CategoryEntity>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(x => x.CategoryId);
            entity.Property(x => x.CategoryName).IsRequired();
            entity.HasIndex(x => x.CategoryName).IsUnique();
        });

        modelBuilder.Entity<TransactionEntity>(entity =>
        {
            entity.ToTable("Transactions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CategoryId).IsRequired();
            entity.HasIndex(x => x.Date);

            entity.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
