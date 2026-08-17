using Invoria.BuildingBlocks.Infrastructure.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Respawn.Graph;

namespace Invoria.Procurement.Application.Tests.ReportPurchaseOrdersCompletedMetrics.Commands;

public class ReportPurchaseOrdersCompletedMetricsTestFixture : ProcurementTestFixture
{
    protected override async Task BeforeAllTestRunAsync()
    {
        await base.BeforeAllTestRunAsync();

        await ResetDatabaseAsync();
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
            TablesToIgnore = new Table[] { "__EFMigrationsHistory" },
            SchemasToExclude = new[] { "Hangfire" }
        });
        await respawner.ResetAsync(connection);
    }
}