using Kakeibo.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kakeibo.Core.Data;

public sealed class KakeiboDbContext : DbContext
{
    public KakeiboDbContext(DbContextOptions<KakeiboDbContext> options) : base(options)
    {
    }

    public DbSet<TransactionEntity> Transactions => Set<TransactionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TransactionEntity>(entity =>
        {
            entity.ToTable("Transactions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Category).IsRequired();
            entity.HasIndex(x => x.Date);
        });
    }
}
