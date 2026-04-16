using Invoria.BuildingBlocks.Application.Extensions;
using Invoria.BuildingBlocks.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Procurement.Infrastructure.Installers
{
    public class ApplicationServiceInstaller : IServiceInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            services
                   .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Invoria.Procurement.Application.AssemblyReference.Assembly))
                   .RegisterFactoriesFromAssembly(Invoria.Procurement.Application.AssemblyReference.Assembly);
        }
    }
}
