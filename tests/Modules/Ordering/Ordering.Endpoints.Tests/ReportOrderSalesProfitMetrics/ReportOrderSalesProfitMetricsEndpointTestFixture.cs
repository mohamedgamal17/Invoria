using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Respawn.Graph;

namespace Invoria.Ordering.Endpoints.Tests.ReportOrderSalesProfitMetrics;

using ReportOrderSalesProfitMetricsEntity = Invoria.Ordering.Domain.Orders.ReportOrderSalesProfitMetrics;

public class ReportOrderSalesProfitMetricsEndpointTestFixture : OrderingTestFixture
{
    protected IOrderingRepository<ReportOrderSalesProfitMetricsEntity> ReportRepository { get; }

    public ReportOrderSalesProfitMetricsEndpointTestFixture()
    {
        ReportRepository = Scope.ServiceProvider.GetRequiredService<IOrderingRepository<ReportOrderSalesProfitMetricsEntity>>();
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