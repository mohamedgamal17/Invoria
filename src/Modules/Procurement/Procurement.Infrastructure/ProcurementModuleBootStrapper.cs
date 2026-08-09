using Invoria.BackgroundJob.Core;
using Invoria.BackgroundJob.Core.Scheduling;
using Invoria.BackgroundJobs.Abstractions;
using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Procurement.Application.Parties.Jobs;
using Invoria.Procurement.Contracts.Events;
using Invoria.Procurement.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Bus;

namespace Invoria.Procurement.Infrastructure
{
    public class ProcurementModuleBootStrapper : IModuleBootstrapper
    {
        public async Task Bootstrap(IServiceProvider serviceProvider)
        {
            var dbContext = serviceProvider.GetRequiredService<ProcurementDbContext>();

            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                await dbContext.Database.MigrateAsync();
            }

            using var scope = serviceProvider.CreateScope();

            var bus = serviceProvider.GetService<IBus>();

            if (bus is not null)
            {
                await bus.Subscribe<PurchaseOrderCompletedIntegrationEvent>();
            }

            var recurringScheduler = scope.ServiceProvider
                .GetRequiredService<IRecurringJobScheduler>();

            recurringScheduler.AddOrUpdate<ReportSupplierMetricsJob>(
                ReportSupplierMetricsJob.Name,
                new IntervalRecurrence(TimeSpan.FromMinutes(
                    RecurringJobIntervalConsts.DefaultReportIntervalInMinutes)));
        }
    }
}