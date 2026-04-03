using Invoria.BuildingBlocks.Core.Extensions;
using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Ordering.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Tests
{
    public class OrderingTestModuleInstaller : IModuleInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            services.InstallModule<OrderingModuleInstaller>(configuration);

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
