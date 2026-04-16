using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Procurement.Domain.Repositories;
using Invoria.Procurement.Infrastructure.EntityFramework;
using Invoria.Procurement.Infrastructure.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Procurement.Infrastructure.Installers
{
    public class EntityFrameworkServiceInstaller : IServiceInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            services.AddInvoriaDbContext<ProcurementDbContext>(cfg =>
            {
                cfg.UseSqlServer(configuration.GetConnectionString("Default"), sqlCfg =>
                    sqlCfg.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                );
            });

            services.AddTransient(typeof(IProcurementRepository<>), typeof(ProcurementRepository<>));
        }
    }
}
