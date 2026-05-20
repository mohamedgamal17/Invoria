using Invoria.Application.Tests;
using Invoria.BuildingBlocks.Core.Extensions;
using Invoria.BuildingBlocks.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Reporting.Application.Tests;

public class ReportingTestFixture : TestFixture
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.InstallModule<ReportingTestModuleInstaller>(Configuration);
    }

    protected override async Task BeforeAllTestRunAsync()
    {
        await ServiceProvider.RunModulesBootstrapperAsync();
    }
}
