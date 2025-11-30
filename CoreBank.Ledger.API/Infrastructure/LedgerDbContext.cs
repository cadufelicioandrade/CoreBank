using CoreBank.Ledger.API.Entity;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace CoreBank.Ledger.API.Infrastructure
{
    public class LedgerDbContext : DbContext
    {
        public LedgerDbContext(DbContextOptions<LedgerDbContext> options)
            : base(options)
        {
        }

        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<AccountBalance> AccountBalances => Set<AccountBalance>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("Transactions");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Type)
                      .HasConversion<byte>()
                      .IsRequired();

                entity.Property(x => x.Operation)
                      .HasConversion<byte>()
                      .IsRequired();

                entity.Property(x => x.Amount)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();

                entity.Property(x => x.BalanceAfter)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();

                entity.Property(x => x.Description)
                      .HasMaxLength(200);

                entity.Property(x => x.CreatedAt)
                      .HasColumnType("datetime2(3)");

                entity.HasIndex(x => new { x.AccountId, x.CreatedAt });

                entity.HasIndex(x => new { x.AccountId, x.CorrelationId })
                      .IsUnique()
                      .HasFilter("[CorrelationId] IS NOT NULL");
            });

            modelBuilder.Entity<AccountBalance>(entity =>
            {
                entity.ToTable("AccountBalances");

                entity.HasKey(x => x.AccountId);

                entity.Property(x => x.CurrentBalance)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();

                entity.Property(x => x.UpdatedAt)
                      .HasColumnType("datetime2(3)");

                entity.Property(x => x.RowVersion)
                      .IsRowVersion();
            });
        }
    }
}
