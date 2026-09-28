using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Financial.Application.Receivables.Consumers;
using Invoria.Financial.Contracts.Receivables.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Handlers;

namespace Invoria.Financial.Infrastructure.Installers;

public sealed class RebusHandlersServiceInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IHandleMessages<CreateOrderReceivableIntegrationEvent>, CreateOrderReceivableIntegrationEventConsumer>();
    }
}
