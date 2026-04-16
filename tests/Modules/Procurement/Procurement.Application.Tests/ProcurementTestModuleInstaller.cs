using Invoria.BuildingBlocks.Core.Extensions;
using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Procurement.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Rebus.Bus;

namespace Invoria.Procurement.Application.Tests
{
    public class ProcurementTestModuleInstaller : IModuleInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging(static b => b.SetMinimumLevel(LogLevel.Warning));

            services.InstallModule<ProcurementModuleInstaller>(configuration);

            services.AddSingleton<Mock<IBus>>(_ =>
            {
                var busMock = new Mock<IBus>();
                busMock
                    .Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
                    .Returns(Task.CompletedTask);
                return busMock;
            });
            services.AddSingleton<IBus>(sp => sp.GetRequiredService<Mock<IBus>>().Object);
        }
    }
}
