using Invoria.BuildingBlocks.Application.Extensions;
using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Ordering.Application.Orders.Services;
using Invoria.Ordering.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Infrastructure.Installers
{
    public class ApplicationServceInstaller : IServiceInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Application.AssemblyReference.Assembly))
                .RegisterFactoriesFromAssembly(Application.AssemblyReference.Assembly);

            services.AddTransient<IOrderNumberGenerator, OrderNumberGenerator>();
        }
    }
}
