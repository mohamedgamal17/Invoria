using FluentValidation;
using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Inventory.Infrastructure.Installers
{
    public class EndpointServiceInstaller : IServiceInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            EndpointsAssemblyRegistry.AddAssembly(Endpoints.AssemblyReference.Assembly);
            services.AddValidatorsFromAssembly(Endpoints.AssemblyReference.Assembly);
        }
    }
}

