using Invoria.BackgroundJobs.Infrastructure.EntityFramework;
using Invoria.BuildingBlocks.Core.Modularity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.BackgroundJobs.Infrastructure;

public class BackgroundJobsModuleBootStrapper : IModuleBootstrapper
{
    public async Task Bootstrap(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BackgroundJobsDbContext>();
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            await dbContext.Database.MigrateAsync();
        }
    }
}
