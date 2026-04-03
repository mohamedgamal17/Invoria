using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Inventory.Application.Batches.Consumers;
using Invoria.Ordering.Contracts.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Handlers;

namespace Invoria.Inventory.Infrastructure.Installers;

public sealed class RebusHandlersServiceInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IHandleMessages<AllocateOrderIntegrationEvent>, AllocateOrderIntegrationEventConsumer>();
        services.AddTransient<IHandleMessages<OrderDispatchedIntegrationEvent>, OrderDispatchedIntegrationEventConsumer>();
    }
}
