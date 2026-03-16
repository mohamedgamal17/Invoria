using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Catalog.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Catalog.Infrastructure
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
