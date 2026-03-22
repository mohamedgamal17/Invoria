using Invoria.BuildingBlocks.Application.Extensions;
using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Catalog.Application.Products.Services;
using Invoria.Catalog.Contracts.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Catalog.Infrastructure.Installers
{
    public class ApplicationServceInstaller : IServiceInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IProductService, ProductService>();

            services
                   .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Application.AssemblyReference.Assembly))
                   .RegisterFactoriesFromAssembly(Application.AssemblyReference.Assembly);
        }
    }
}
