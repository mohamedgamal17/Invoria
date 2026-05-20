using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Ordering.Contracts.Events;
using Invoria.Reporting.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Bus;

namespace Invoria.Reporting.Infrastructure
{
    public class ReportingModuleBootStrapper : IModuleBootstrapper
    {
        public async Task Bootstrap(IServiceProvider serviceProvider)
        {
            var dbContext = serviceProvider.GetRequiredService<ReportingDbContext>();

            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                await dbContext.Database.MigrateAsync();
            }

            var bus = serviceProvider.GetService<IBus>();
            if (bus is not null)
            {
                await bus.Subscribe<OrderCreatedIntegrationEvent>();
                await bus.Subscribe<OrderUpdatedIntegrationEvent>();
            }
        }
    }
}
