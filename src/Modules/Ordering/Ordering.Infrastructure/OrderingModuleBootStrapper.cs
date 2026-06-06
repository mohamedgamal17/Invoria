using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.Ordering.Application.Orders.Sagas.Activities;
using Invoria.Ordering.Contracts.Orders.Events;
using Invoria.Ordering.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Bus;

namespace Invoria.Ordering.Infrastructure
{
    public class OrderingModuleBootStrapper : IModuleBootstrapper
    {
        public async Task Bootstrap(IServiceProvider serviceProvider)
        {
            var dbcontext = serviceProvider.GetRequiredService<OrderingDbContext>();

            await dbcontext.Database.MigrateAsync();


            var bus = serviceProvider.GetService<IBus>();

            if (bus is not null)
            {
                await bus.Subscribe<OrderCreatedIntegrationEvent>();
                await bus.Subscribe<OrderAcceptedIntegrationEvent>();
                await bus.Subscribe<AllocationCreatedIntegrationEvent>();
                await bus.Subscribe<AllocationFailedIntegrationEvent>();
                await bus.Subscribe<RecordOrderAllocationSagaActivity>();
                await bus.Subscribe<ReviseOrderSagaActivity>();
                await bus.Subscribe<ReleaseOrderAllocationsIntegrationEvent>();
            }
        }
    }
}
