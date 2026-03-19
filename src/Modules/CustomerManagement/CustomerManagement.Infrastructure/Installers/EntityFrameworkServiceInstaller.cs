using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.CustomerManagement.Infrastructure.EntityFramework;
using Invoria.CustomerManagement.Domain.Customers;
using Invoria.CustomerManagement.Infrastructure.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.CustomerManagement.Infrastructure.Installers
{
    public class EntityFrameworkServiceInstaller : IServiceInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            services.AddInvoriaDbContext<CustomerManagementDbContext>(cfg =>
            {
                cfg.UseSqlServer(configuration.GetConnectionString("Default"), sqlCfg =>
                    sqlCfg.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                );
            });

            services.AddTransient(typeof(ICustomerRepository<>), typeof(CustomerRepository<>));
        }
    }
}

