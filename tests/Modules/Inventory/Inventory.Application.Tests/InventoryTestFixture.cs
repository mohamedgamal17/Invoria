using Invoria.Application.Tests;
using Invoria.BuildingBlocks.Core.Extensions;
using Invoria.BuildingBlocks.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Rebus.Bus;
using System.Threading.Tasks;

namespace Invoria.Inventory.Application.Tests
{
    public class InventoryTestFixture : TestFixture
    {
        protected override void ConfigureServices(IServiceCollection services)
        {
            services.InstallModule<InventoryTestModuleInstaller>(Configuration);
            services.AddLogging(static b => b.SetMinimumLevel(LogLevel.Warning));

            var bus = new Mock<IBus>();
            bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
                .Returns(Task.CompletedTask);
            bus.Setup(b => b.Subscribe(It.IsAny<Type>())).Returns(Task.CompletedTask);
            services.AddSingleton(bus.Object);
        }

        protected override async Task BeforeAllTestRunAsync()
        {
            await ServiceProvider.RunModulesBootstrapperAsync();
        }
    }
}

