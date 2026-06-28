using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Inventory.Application.Allocations.Consumers;
using Invoria.Inventory.Application.Batches.Consumers;
using Invoria.Inventory.Application.Returns.Consumers;
using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.Inventory.Contracts.Returns.Events;
using Invoria.Ordering.Contracts.Orders.Events;
using Invoria.Procurement.Contracts.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Handlers;

namespace Invoria.Inventory.Infrastructure.Installers;

public sealed class RebusHandlersServiceInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IHandleMessages<AllocateOrderIntegrationEvent>, AllocateOrderIntegrationEventConsumer>();
        services.AddTransient<IHandleMessages<ReleaseOrderAllocationsIntegrationEvent>, ReleaseOrderAllocationsIntegrationEventConsumer>();
        services.AddTransient<IHandleMessages<PurchaseOrderCompletedIntegrationEvent>, PurchaseOrderCompletedIntegrationEventConsumer>();
        services.AddTransient<IHandleMessages<AllocationCreatedIntegrationEvent>, AllocationCreatedIntegrationEventConsumer>();
        services.AddTransient<IHandleMessages<ReleaseAllocationIntegrationEvent>, ReleaseAllocationIntegrationEventConsumer>();
        services.AddTransient<IHandleMessages<CreateImmediateReturnIntegrationEvent>, CreateImmediateReturnIntegrationEventConsumer>();
        services.AddTransient<IHandleMessages<ProcessImmediateReturnIntegrationEvent>, ProcessImmediateReturnIntegrationEventConsumer>();
        services.AddTransient<IHandleMessages<OrderCompletedIntegrationEvent>, OrderCompletedIntegrationEventConsumer>();
    }
}
