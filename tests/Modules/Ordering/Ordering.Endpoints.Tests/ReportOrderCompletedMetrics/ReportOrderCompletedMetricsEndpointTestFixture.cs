using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Respawn.Graph;

namespace Invoria.Ordering.Endpoints.Tests.ReportOrderCompletedMetrics;

using ReportOrderCompletedMetricsEntity = Invoria.Ordering.Domain.Orders.ReportOrderCompletedMetrics;

public class ReportOrderCompletedMetricsEndpointTestFixture : OrderingTestFixture
{
    protected IOrderingRepository<ReportOrderCompletedMetricsEntity> ReportRepository { get; }

    public ReportOrderCompletedMetricsEndpointTestFixture()
    {
        ReportRepository = Scope.ServiceProvider.GetRequiredService<IOrderingRepository<ReportOrderCompletedMetricsEntity>>();
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