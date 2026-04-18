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
            var firstOrderNumber = await generator.GenerateAsync();
            var secondOrderNumber = await generator.GenerateAsync();
            var thirdOrderNumber = await generator.GenerateAsync();

            firstOrderNumber.Length.Should().Be(10);
            secondOrderNumber.Length.Should().Be(10);
            thirdOrderNumber.Length.Should().Be(10);

            secondOrderNumber.Should().NotBe(firstOrderNumber);
            thirdOrderNumber.Should().NotBe(firstOrderNumber);
            thirdOrderNumber.Should().NotBe(secondOrderNumber);

            var firstSequence = int.Parse(firstOrderNumber[6..]);
            var secondSequence = int.Parse(secondOrderNumber[6..]);
            var thirdSequence = int.Parse(thirdOrderNumber[6..]);

            secondSequence.Should().Be(firstSequence + 1);
            thirdSequence.Should().Be(secondSequence + 1);
        }
    }
}
