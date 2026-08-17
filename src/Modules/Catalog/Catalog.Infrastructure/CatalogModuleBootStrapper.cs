using Invoria.BackgroundJob.Core;
using Invoria.BackgroundJob.Core.Scheduling;
using Invoria.BackgroundJobs.Abstractions;
using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Catalog.Application.Products.Jobs;
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

            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                await dbContext.Database.MigrateAsync();
            }

            using var scope = serviceProvider.CreateScope();

            var recurringScheduler = scope.ServiceProvider
                .GetRequiredService<IRecurringJobScheduler>();

            recurringScheduler.AddOrUpdate<ReportProductMetricsJob>(
                ReportProductMetricsJob.Name,
                new IntervalRecurrence(TimeSpan.FromMinutes(
                    RecurringJobIntervalConsts.DefaultReportIntervalInMinutes)));
        }
    }
}
