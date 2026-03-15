using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Modules.Catalog.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Modules.Catalog.Infrastructure
{
    public class CatalogModuleBootStrapper : IModuleBootstrapper
    {
        public async Task Bootstrap(IServiceProvider serviceProvider)
        {
            var dbContext = serviceProvider.GetRequiredService<CatalogDbContext>();

            await dbContext.Database.MigrateAsync();

        }
    }
}
