using Invoria.Application.Tests.Utilites;
using Invoria.BuildingBlocks.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Invoria.Application.Tests
{
    public class ApplicationTestModuleInstaller : IModuleInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging()
               .AddTransient<ILogger, TestOutputLogger>()
               .AddSingleton<ILoggerFactory>(provider => new TestOutputLoggerFactory(true));

        }
    }
}
