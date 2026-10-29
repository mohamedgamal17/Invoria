using Invoria.BackgroundJobs.Test;
using Invoria.BuildingBlocks.Core.Extensions;
using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.CustomerManagement.Contracts.Services;
using Invoria.Financial.Application.Tests.Fakes;
using Invoria.Financial.Infrastructure;
using Invoria.Ordering.Contracts.Orders.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Rebus.Bus;

namespace Invoria.Financial.Application.Tests
{
    public class FinancialTestModuleInstaller : IModuleInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging(static b => b.SetMinimumLevel(LogLevel.Warning));

            services.InstallModule<FinancialModuleInstaller>(configuration);

            services.InstallModule<BackgroundJobsTestModuleInstaller>(configuration);

            services.AddSingleton<IOrderService, EmptyListOrderService>();

            services.AddSingleton<ICustomerService, EmptyListCustomerService>();

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
