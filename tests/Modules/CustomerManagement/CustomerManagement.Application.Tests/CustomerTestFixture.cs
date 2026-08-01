using Invoria.Application.Tests;
using Invoria.BuildingBlocks.Core.Extensions;
using Invoria.BuildingBlocks.Infrastructure.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Respawn.Graph;
using System.Threading.Tasks;

namespace Invoria.CustomerManagement.Application.Tests
{
    public class CustomerTestFixture : TestFixture
    {
        protected override void ConfigureServices(IServiceCollection services)
        {
            services.InstallModule<CustomerTestModuleInstaller>(Configuration);
        }

        protected override async Task BeforeAllTestRunAsync()
        {
            await ServiceProvider.RunModulesBootstrapperAsync();
        }

        protected override async Task AfterAllTestTearDown()
        {
            await base.AfterAllTestTearDown();

            await ResetDatabaseAsync();
        }

        private async Task ResetDatabaseAsync()
        {
            var connectionString = Configuration.GetConnectionString("Default");

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            var respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
            {
                TablesToIgnore = new Table[] { "__EFMigrationsHistory" }
            });
            await respawner.ResetAsync(connection);
        }
    }
}

