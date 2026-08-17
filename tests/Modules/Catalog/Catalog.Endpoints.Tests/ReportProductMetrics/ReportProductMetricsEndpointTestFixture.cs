using Invoria.Catalog.Domain;
using Invoria.Catalog.Domain.Products;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Respawn.Graph;

namespace Invoria.Catalog.Endpoints.Tests.ReportProductMetrics
{
    using ReportProductMetricsEntity = Invoria.Catalog.Domain.Products.ReportProductMetrics;

    public class ReportProductMetricsEndpointTestFixture : CatalogTestFixture
    {
        protected ICatalogRepository<Product> ProductRepository { get; }
        protected ICatalogRepository<ReportProductMetricsEntity> ReportRepository { get; }

        public ReportProductMetricsEndpointTestFixture()
        {
            ProductRepository = Scope.ServiceProvider.GetRequiredService<ICatalogRepository<Product>>();
            ReportRepository = Scope.ServiceProvider.GetRequiredService<ICatalogRepository<ReportProductMetricsEntity>>();
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
