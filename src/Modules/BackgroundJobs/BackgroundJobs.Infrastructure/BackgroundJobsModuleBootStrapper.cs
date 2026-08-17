using Hangfire.SqlServer;
using Invoria.BackgroundJobs.Infrastructure.EntityFramework;
using Invoria.BuildingBlocks.Core.Modularity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.BackgroundJobs.Infrastructure;

public class BackgroundJobsModuleBootStrapper : IModuleBootstrapper
{
    private const string HangfireSchemaName = "Hangfire";

    public async Task Bootstrap(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("Default");

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            await EnsureDatabaseExistsAsync(connectionString);
            EnsureHangfireSchemaExists(connectionString);
        }

        var dbContext = scope.ServiceProvider.GetRequiredService<BackgroundJobsDbContext>();
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            await dbContext.Database.MigrateAsync();
        }
    }

    private static async Task EnsureDatabaseExistsAsync(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(builder.InitialCatalog))
        {
            return;
        }

        var databaseName = builder.InitialCatalog;
        builder.InitialCatalog = "master";

        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync();

        var escapedName = databaseName.Replace("]", "]]");
        var sql = $"IF DB_ID(N'{databaseName.Replace("'", "''")}') IS NULL EXEC(N'CREATE DATABASE [{escapedName}]');";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    private static void EnsureHangfireSchemaExists(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        EnsureHangfireSchemaVersion(connection);

        SqlServerObjectsInstaller.Install(connection, HangfireSchemaName, false);
    }

    private static void EnsureHangfireSchemaVersion(SqlConnection connection)
    {
        var schemaExists = ExecuteScalar<int>(connection,
            $"SELECT COUNT(1) FROM sys.schemas WHERE name = N'{HangfireSchemaName}'") > 0;
        if (!schemaExists)
        {
            return;
        }

        var versionTableExists = ExecuteScalar<int>(connection,
            $"SELECT COUNT(1) FROM sys.tables WHERE SCHEMA_NAME(schema_id) = N'{HangfireSchemaName}' AND name = N'Schema'") > 0;
        if (!versionTableExists)
        {
            return;
        }

        var hasVersion = ExecuteScalar<int>(connection,
            $"SELECT COUNT(1) FROM [{HangfireSchemaName}].[Schema]") > 0;
        if (hasVersion)
        {
            return;
        }

        using var insertCommand = connection.CreateCommand();
        insertCommand.CommandText =
            $"INSERT INTO [{HangfireSchemaName}].[Schema] ([Version]) VALUES (@version)";
        insertCommand.Parameters.AddWithValue("@version", SqlServerObjectsInstaller.LatestSchemaVersion);
        insertCommand.ExecuteNonQuery();
    }

    private static T ExecuteScalar<T>(SqlConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return (T)command.ExecuteScalar();
    }
}