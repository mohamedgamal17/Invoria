using Invoria.BackgroundJob.Core;
using Invoria.BackgroundJob.Core.Scheduling;
using Invoria.BackgroundJobs.Abstractions;
using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.CustomerManagement.Application.ReportCustomerMetrics.Jobs;
using Invoria.CustomerManagement.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.CustomerManagement.Infrastructure
{
    public class CustomerManagementModuleBootStrapper : IModuleBootstrapper
    {
        public async Task Bootstrap(IServiceProvider serviceProvider)
        {
            var dbContext = serviceProvider.GetRequiredService<CustomerManagementDbContext>();

            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                await dbContext.Database.MigrateAsync();
            }

            using var scope = serviceProvider.CreateScope();

            var recurringScheduler = scope.ServiceProvider
                .GetRequiredService<IRecurringJobScheduler>();

            recurringScheduler.AddOrUpdate<ReportCustomerMetricsJob>(
                ReportCustomerMetricsJob.Name,
                new IntervalRecurrence(TimeSpan.FromMinutes(
                    RecurringJobIntervalConsts.DefaultReportIntervalInMinutes)));
        }
    }
}

