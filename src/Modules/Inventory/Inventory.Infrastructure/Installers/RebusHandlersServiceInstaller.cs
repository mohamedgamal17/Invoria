using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Inventory.Infrastructure.Events;
using Invoria.Ordering.Contracts.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Handlers;

namespace Invoria.Inventory.Infrastructure.Installers;

public sealed class RebusHandlersServiceInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<AllocateOrderIntegrationEventHandler>();
        services.AddTransient<IHandleMessages<AllocateOrderIntegrationEvent>, AllocateOrderIntegrationEventHandler>();
    }
}
