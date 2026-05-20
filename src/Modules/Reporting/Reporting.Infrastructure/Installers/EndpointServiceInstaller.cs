using FluentValidation;
using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Reporting.Infrastructure.Installers
{
    public class EndpointServiceInstaller : IServiceInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            EndpointsAssemblyRegistry.AddAssembly(Invoria.Reporting.Endpoints.AssemblyReference.Assembly);
            services.AddValidatorsFromAssembly(Invoria.Reporting.Endpoints.AssemblyReference.Assembly);
        }
    }
}
