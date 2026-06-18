using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Ordering.Contracts.Orders.Events;
using Invoria.Reporting.Application.Orders.Consumers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Handlers;

namespace Invoria.Reporting.Infrastructure.Installers;

public sealed class RebusHandlersServiceInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IHandleMessages<OrderCreatedIntegrationEvent>, OrderCreatedIntegrationEventConsumer>();
        services.AddTransient<IHandleMessages<OrderUpdatedIntegrationEvent>, OrderUpdatedIntegrationEventConsumer>();
    }
}
