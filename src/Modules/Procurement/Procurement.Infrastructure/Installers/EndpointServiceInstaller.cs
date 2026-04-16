using FluentValidation;
using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Procurement.Infrastructure.Installers
{
    public class EndpointServiceInstaller : IServiceInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            EndpointsAssemblyRegistry.AddAssembly(Invoria.Procurement.Endpoints.AssemblyReference.Assembly);
            services.AddValidatorsFromAssembly(Invoria.Procurement.Endpoints.AssemblyReference.Assembly);
        }
    }
}
