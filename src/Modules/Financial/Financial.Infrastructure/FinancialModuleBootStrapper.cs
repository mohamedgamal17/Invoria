using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Financial.Contracts.Receivables.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Invoria.Financial.Infrastructure.EntityFramework;
using Rebus.Bus;

namespace Invoria.Financial.Infrastructure
{
    public class FinancialModuleBootStrapper : IModuleBootstrapper
    {
        public async Task Bootstrap(IServiceProvider serviceProvider)
        {
            var dbContext = serviceProvider.GetRequiredService<FinancialDbContext>();

            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                await dbContext.Database.MigrateAsync();
            }

            var bus = serviceProvider.GetService<IBus>();

            if (bus is not null)
            {
                await bus.Subscribe<CreateOrderReceivableIntegrationEvent>();
            }
        }
    }
}
