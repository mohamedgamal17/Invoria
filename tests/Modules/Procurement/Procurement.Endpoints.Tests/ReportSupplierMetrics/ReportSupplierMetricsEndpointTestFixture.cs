using Invoria.Procurement.Domain.Parties;
using Invoria.Procurement.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Respawn.Graph;

namespace Invoria.Procurement.Endpoints.Tests.ReportSupplierMetrics;

using ReportSupplierMetricsEntity = Invoria.Procurement.Domain.Parties.ReportSupplierMetrics;

public class ReportSupplierMetricsEndpointTestFixture : ProcurementTestFixture
{
    protected IProcurementRepository<Supplier> SupplierRepository { get; }
    protected IProcurementRepository<ReportSupplierMetricsEntity> ReportRepository { get; }

    public ReportSupplierMetricsEndpointTestFixture()
    {
        SupplierRepository = Scope.ServiceProvider.GetRequiredService<IProcurementRepository<Supplier>>();
        ReportRepository = Scope.ServiceProvider.GetRequiredService<IProcurementRepository<ReportSupplierMetricsEntity>>();
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