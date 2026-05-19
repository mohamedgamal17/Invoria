using Invoria.BuildingBlocks.EntityFramework.Contexts;
using Invoria.BuildingBlocks.EntityFramework.Hooks;
using Invoria.Reporting.Domain.Orders;
using Invoria.Reporting.Domain.Orders.OrderPeriodSummary;
using Invoria.Reporting.Domain.Orders.StatusSummary;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Invoria.Reporting.Infrastructure.EntityFramework
{
    public class ReportingDbContext : InvoriaDbContext<ReportingDbContext>
    {
        public DbSet<ReportedOrder> ReportedOrders => Set<ReportedOrder>();

        public DbSet<ReportedOrderLine> ReportedOrderLines => Set<ReportedOrderLine>();

        public DbSet<ReportedOrderPayment> ReportedOrderPayments => Set<ReportedOrderPayment>();

        public DbSet<ReportedOrderStateTransition> ReportedOrderStateTransitions => Set<ReportedOrderStateTransition>();

        public DbSet<ReportedOrderFailureDetail> ReportedOrderFailureDetails => Set<ReportedOrderFailureDetail>();

        public DbSet<ReportedOrderStatusByDay> ReportedOrderStatusByDays => Set<ReportedOrderStatusByDay>();

        public DbSet<OrderPeriodSummary> OrderPeriodSummaries => Set<OrderPeriodSummary>();

        public ReportingDbContext(DbContextOptions<ReportingDbContext> options, IDbHookEngine dbHookEngine) : base(options, dbHookEngine)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
    }
}
