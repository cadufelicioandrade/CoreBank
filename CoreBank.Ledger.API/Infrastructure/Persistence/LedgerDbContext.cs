using CoreBank.Ledger.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreBank.Ledger.API.Infrastructure.Persistence
{
    public class LedgerDbContext : DbContext
    {
        public LedgerDbContext(DbContextOptions<LedgerDbContext> options) : base(options)
        {
        }

        // Transações de ledger
        public DbSet<LedgerTransaction> Transactions { get; set; } = null!;

        // 👉 Registro de idempotência
        public DbSet<IdempotentRequest> IdempotentRequests { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mapeia LedgerTransaction (se já tiver, mantém)
            modelBuilder.Entity<LedgerTransaction>(cfg =>
            {
                cfg.HasKey(t => t.Id);
                cfg.Property(t => t.AccountNumber).HasMaxLength(50).IsRequired();
                cfg.Property(t => t.Amount).HasColumnType("decimal(18,2)");
            });

            // Mapeia IdempotentRequest
            modelBuilder.Entity<IdempotentRequest>(cfg =>
            {
                cfg.HasKey(x => x.Id);
                cfg.Property(x => x.Endpoint).HasMaxLength(200).IsRequired();
                cfg.Property(x => x.CreatedAt).IsRequired();
            });
        }
    }
}
