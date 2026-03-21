using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Ordering.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Infrastructure
{
    public class OrderingModuleBootStrapper : IModuleBootstrapper
    {
        public async Task Bootstrap(IServiceProvider serviceProvider)
        {
            var dbcontext = serviceProvider.GetRequiredService<OrderingDbContext>();

            await dbcontext.Database.MigrateAsync();
        }
    }
}
