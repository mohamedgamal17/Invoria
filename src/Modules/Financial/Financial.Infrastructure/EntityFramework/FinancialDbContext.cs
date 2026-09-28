using Invoria.BuildingBlocks.EntityFramework.Contexts;
using Invoria.BuildingBlocks.EntityFramework.Hooks;
using Invoria.Financial.Domain.Receivables;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Invoria.Financial.Infrastructure.EntityFramework
{
    public class FinancialDbContext : InvoriaDbContext<FinancialDbContext>
    {
        public FinancialDbContext(DbContextOptions<FinancialDbContext> options, IDbHookEngine dbHookEngine) : base(options, dbHookEngine)
        {
        }

        public DbSet<Receivable> Receivables => Set<Receivable>();

        public DbSet<ReceivableSettlement> ReceivableSettlements => Set<ReceivableSettlement>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
    }
}
