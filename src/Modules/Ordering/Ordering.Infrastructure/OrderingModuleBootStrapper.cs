using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.Inventory.Contracts.Returns.Events;
using Invoria.Ordering.Application.Invoices.Sagas.Activities;
using Invoria.Ordering.Application.Orders.Sagas.Activities;
using Invoria.Ordering.Contracts.Invoices.Events;
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
                await bus.Subscribe<AllocationSucceededIntegrationEvent>();
                await bus.Subscribe<AllocationReleasedIntegrationEvent>();
                await bus.Subscribe<OrderRevisionRequestedIntegrationEvent>();
                await bus.Subscribe<OrderCompletedIntegrationEvent>();
                await bus.Subscribe<RecordOrderAllocationSagaActivity>();
                await bus.Subscribe<ReviseOrderSagaActivity>();
                await bus.Subscribe<MarkOrderAllocatedSagaActivity>();
                await bus.Subscribe<CreateOrderReturnSagaActivity>();
                await bus.Subscribe<CreateOrderInvoiceSagaActivity>();
                await bus.Subscribe<OrderReturnRequestedIntegrationEvent>();
                await bus.Subscribe<ImmediateReturnCreatedIntegrationEvent>();
                await bus.Subscribe<RecordOrderReturnSagaActivity>();
                await bus.Subscribe<OrderInvoiceRequestIntegrationEvent>();
                await bus.Subscribe<OrderInvoiceCreatedIntegrationEvent>();
                await bus.Subscribe<RecordOrderInvoiceSagaActivity>();
                await bus.Subscribe<CreateOrderInvoiceIntegrationEvent>();
            }
        }
    }
}
