using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Ordering.Application.Orders.Consumers;
using Invoria.Ordering.Contracts.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Handlers;

namespace Invoria.Ordering.Infrastructure.Installers;

public sealed class RebusHandlersServiceInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IHandleMessages<OrderAllocationSucceededIntegrationEvent>, OrderAllocationSucceededIntegrationEventConsumer>();
        services.AddTransient<IHandleMessages<OrderReopenInventoryReleasedIntegrationEvent>, OrderReopenInventoryReleasedIntegrationEventConsumer>();
        services.AddTransient<IHandleMessages<OrderRefusalInventoryReleasedIntegrationEvent>, OrderRefusalInventoryReleasedIntegrationEventConsumer>();
    }
}
