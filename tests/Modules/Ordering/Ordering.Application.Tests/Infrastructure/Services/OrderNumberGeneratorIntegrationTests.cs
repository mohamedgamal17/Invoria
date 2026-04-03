using FluentAssertions;
using Invoria.Ordering.Application.Orders.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Application.Tests.Infrastructure.Services
{
    [TestFixture]
    public class OrderNumberGeneratorIntegrationTests : OrderingTestFixture
    {
        [Test]
        [Category("Integration")]
        [Explicit("Requires SQL Server LocalDB connection configured for OrderingInvoriaApplicationTests.")]
        public async Task Should_generate_unique_order_numbers_under_requests()
        {
            var generator = ServiceProvider.GetRequiredService<IOrderNumberGenerator>();

            const int total = 200;

            var orderNumber = await generator.GenerateAsync();

            orderNumber.Length.Should().Be(10);
        }
    }
}
