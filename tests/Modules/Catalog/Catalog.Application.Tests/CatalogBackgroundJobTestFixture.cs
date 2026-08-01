using Invoria.Application.Tests;
using Invoria.BackgroundJobs.Test;
using Invoria.BuildingBlocks.Core.Extensions;
using Invoria.BuildingBlocks.Infrastructure.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Respawn.Graph;

namespace Invoria.Catalog.Application.Tests;

public class CatalogBackgroundJobTestFixture : BackgroundJobTestFixture
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.InstallModule<CatalogTestModuleInstaller>(Configuration);
    }

    protected override async Task BeforeAllTestRunAsync()
    {
        await ServiceProvider.RunModulesBootstrapperAsync();

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
            TablesToIgnore = new Table[] { "__EFMigrationsHistory" }
        });
        await respawner.ResetAsync(connection);
    }
}
