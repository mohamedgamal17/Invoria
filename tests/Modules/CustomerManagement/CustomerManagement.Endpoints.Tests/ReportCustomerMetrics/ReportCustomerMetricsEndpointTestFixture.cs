using Invoria.CustomerManagement.Domain.Customers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Respawn.Graph;

namespace Invoria.CustomerManagement.Endpoints.Tests.ReportCustomerMetrics
{
    using ReportCustomerMetricsEntity = Invoria.CustomerManagement.Domain.Customers.ReportCustomerMetrics;

    public class ReportCustomerMetricsEndpointTestFixture : CustomerTestFixture
    {
        protected ICustomerRepository<Customer> CustomerRepository { get; }
        protected ICustomerRepository<ReportCustomerMetricsEntity> ReportRepository { get; }

        public ReportCustomerMetricsEndpointTestFixture()
        {
            CustomerRepository = Scope.ServiceProvider.GetRequiredService<ICustomerRepository<Customer>>();
            ReportRepository = Scope.ServiceProvider.GetRequiredService<ICustomerRepository<ReportCustomerMetricsEntity>>();
        }

        [SetUp]
        public async Task SetUpResetAsync()
        {
            await ResetDatabaseAsync();
        }

        [TearDown]
        public async Task TearDownResetAsync()
        {
            await ResetDatabaseAsync();
        }

        protected async Task ResetDatabaseAsync()
        {
            var configuration = Scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("Default");

            using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
            await connection.OpenAsync();
            var respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
            {
                TablesToIgnore = new Table[] { "__EFMigrationsHistory" },
                SchemasToExclude = new[] { "Hangfire" }
            });
            await respawner.ResetAsync(connection);
        }
    }
}