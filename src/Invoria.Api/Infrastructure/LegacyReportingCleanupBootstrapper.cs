using Invoria.BuildingBlocks.Core.Modularity;
using Microsoft.Data.SqlClient;

namespace Invoria.Api.Infrastructure;

public sealed class LegacyReportingCleanupBootstrapper : IModuleBootstrapper
{
    private const string ConnectionStringName = "Default";

    private static readonly string[] TablesToDrop =
    [
        "ReportedOrderPayment",
        "ReportedOrderLine",
        "ReportedOrderStatusByDay",
        "OrderPeriodSummary",
        "DebtSummary",
        "ReportedOrder"
    ];

    public async Task Bootstrap(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString(ConnectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        if (!await ReportingTableExists(connection))
        {
            return;
        }

        await using var transaction = connection.BeginTransaction();

        try
        {
            foreach (var tableName in TablesToDrop)
            {
                var sql = $"IF OBJECT_ID('dbo.{tableName}', 'U') IS NOT NULL DROP TABLE dbo.{tableName}";
                await using var command = new SqlCommand(sql, connection, transaction);
                await command.ExecuteNonQueryAsync();
            }

            var deleteMigrationHistory = "DELETE FROM dbo.[__EFMigrationsHistory] WHERE MigrationId LIKE '%Reporting%'";
            await using var deleteCommand = new SqlCommand(deleteMigrationHistory, connection, transaction);
            await deleteCommand.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static async Task<bool> ReportingTableExists(SqlConnection connection)
    {
        var sql = "SELECT COUNT(1) FROM sys.tables WHERE name = 'ReportedOrder'";
        await using var command = new SqlCommand(sql, connection);
        var result = await command.ExecuteScalarAsync() as int? ?? 0;
        return result > 0;
    }
}
