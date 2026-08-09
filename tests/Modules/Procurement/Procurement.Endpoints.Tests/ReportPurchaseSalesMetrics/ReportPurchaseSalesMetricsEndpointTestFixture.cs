using Invoria.Procurement.Domain.PurchaseOrders;
using Invoria.Procurement.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Respawn.Graph;

namespace Invoria.Procurement.Endpoints.Tests.ReportPurchaseSalesMetrics;

using ReportPurchaseSalesMetricsEntity = Invoria.Procurement.Domain.PurchaseOrders.ReportPurchaseSalesMetrics;

public class ReportPurchaseSalesMetricsEndpointTestFixture : ProcurementTestFixture
{
    protected IProcurementRepository<ReportPurchaseSalesMetricsEntity> ReportRepository { get; }

    public ReportPurchaseSalesMetricsEndpointTestFixture()
    {
        ReportRepository = Scope.ServiceProvider.GetRequiredService<IProcurementRepository<ReportPurchaseSalesMetricsEntity>>();
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