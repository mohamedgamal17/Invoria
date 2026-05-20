using Invoria.BuildingBlocks.Core.Extensions;
using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Reporting.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Invoria.Reporting.Application.Tests;

public sealed class ReportingTestModuleInstaller : IModuleInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging(static b => b.SetMinimumLevel(LogLevel.Warning));
        services.InstallModule<ReportingModuleInstaller>(configuration);
    }
}
