using FluentAssertions;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Infrastructure.Services;

namespace Invoria.Ordering.Application.Tests.Services
{
    [TestFixture]
    public class OrderNumberGeneratorTests
    {
        [Test]
        public async Task Should_generate_order_number_with_utc_yyMMdd_and_zero_padded_sequence()
        {
            var generator = new OrderNumberGenerator(
                new StubCounterRepository(1),
                () => new DateTime(2026, 03, 21, 10, 15, 00, DateTimeKind.Utc));

            var orderNumber = await generator.GenerateAsync(CancellationToken.None);

            orderNumber.Should().Be("2603210001");
        }

        [Test]
        public async Task Should_throw_clear_exception_when_daily_counter_exceeds_limit()
        {
            var generator = new OrderNumberGenerator(
                new StubCounterRepository(10000),
                () => new DateTime(2026, 03, 21, 10, 15, 00, DateTimeKind.Utc));

            var action = async () => await generator.GenerateAsync(CancellationToken.None);

            await action.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Daily order number limit exceeded for date 260321. Maximum allowed is 9999.");
        }

        private sealed class StubCounterRepository : ICounterRepository
        {
            private readonly int _value;

            public StubCounterRepository(int value)
            {
                _value = value;
            }

            public Task<int> IncrementDailyCounterAsync(DateOnly date, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(_value);
            }
        }
    }
}
