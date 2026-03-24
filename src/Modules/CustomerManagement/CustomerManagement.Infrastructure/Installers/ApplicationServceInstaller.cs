using Invoria.BuildingBlocks.Application.Extensions;
using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.CustomerManagement.Application.Customers.Services;
using Invoria.CustomerManagement.Contracts.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.CustomerManagement.Infrastructure.Installers
{
    public class ApplicationServceInstaller : IServiceInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ICustomerService, CustomerService>();

            services
                   .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Application.AssemblyReference.Assembly))
                   .RegisterFactoriesFromAssembly(Application.AssemblyReference.Assembly);
        }
    }
}

