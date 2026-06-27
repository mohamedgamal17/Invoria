using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.Inventory.Contracts.Returns.Events;
using Invoria.Inventory.Infrastructure.EntityFramework;
using Invoria.Ordering.Contracts.Orders.Events;
using Invoria.Procurement.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Bus;

namespace Invoria.Inventory.Infrastructure
{
    public class InventoryModuleBootStrapper : IModuleBootstrapper
    {
        public async Task Bootstrap(IServiceProvider serviceProvider)
        {
            var dbContext = serviceProvider.GetRequiredService<InventoryDbContext>();

            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                await dbContext.Database.MigrateAsync();
            }

            var bus = serviceProvider.GetService<IBus>();

            if (bus is not null)
            {
                await bus.Subscribe<AllocateOrderIntegrationEvent>();
                await bus.Subscribe<AllocationCreatedIntegrationEvent>();
                await bus.Subscribe<ReleaseAllocationIntegrationEvent>();
                await bus.Subscribe<CreateImmediateReturnIntegrationEvent>();
                await bus.Subscribe<ProcessImmediateReturnIntegrationEvent>();
                await bus.Subscribe<ReleaseOrderAllocationsIntegrationEvent>();
                await bus.Subscribe<PurchaseOrderCompletedIntegrationEvent>();
            }
        }
    }
}

