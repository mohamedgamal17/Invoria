using Invoria.BuildingBlocks.EntityFramework.Contexts;
using Invoria.BuildingBlocks.EntityFramework.Hooks;
using Invoria.Procurement.Domain.Parties;
using Invoria.Procurement.Domain.PurchaseOrders;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Invoria.Procurement.Infrastructure.EntityFramework;

public class ProcurementDbContext : InvoriaDbContext<ProcurementDbContext>
{
    public ProcurementDbContext(DbContextOptions<ProcurementDbContext> options, IDbHookEngine dbHookEngine)
        : base(options, dbHookEngine)
    {
    }

    public DbSet<Supplier> Suppliers => Set<Supplier>();

    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();

    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();

    public DbSet<PurchaseStateHistory> PurchaseStateHistory => Set<PurchaseStateHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
