using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Ordering.Contracts.Events;
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
                await bus.Subscribe<AllocateOrderIntegrationEvent>();
                await bus.Subscribe<OrderAllocationSucceededIntegrationEvent>();
                await bus.Subscribe<OrderAllocationFailedIntegrationEvent>();
                await bus.Subscribe<OrderDispatchedIntegrationEvent>();
                await bus.Subscribe<ReleaseOrderAllocationsIntegrationEvent>();
                await bus.Subscribe<OrderReopenInventoryReleasedIntegrationEvent>();
                await bus.Subscribe<OrderRefusalInventoryReleasedIntegrationEvent>();
            }
        }
    }
}
