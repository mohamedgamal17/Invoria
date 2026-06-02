using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Ordering.Application.Orders.Sagas;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Infrastructure.Installers;

public sealed class RebusHandlersServiceInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<OrderSaga>();
    }
}
