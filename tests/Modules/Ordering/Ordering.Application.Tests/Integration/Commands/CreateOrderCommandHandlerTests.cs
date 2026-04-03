using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.Ordering.Application.Orders.Commands.CreateOrder;
using Invoria.Ordering.Application.Tests.Assertions;

namespace Invoria.Ordering.Application.Tests.Integration.Commands
{
    [TestFixture]
    public class CreateOrderCommandHandlerTests : OrderTestFixture
    {
        [Test]
        public async Task Should_create_order_and_return_order_dto()
        {
            var command = new CreateOrderCommand(
                Guid.NewGuid().ToString(),
                new List<CreateOrderItemCommand>
                {
                    new(Guid.NewGuid().ToString(), 2, 10m),
                    new(Guid.NewGuid().ToString(), 1, 25m)
                });

            var result = await Mediator.Send(command);

            result.ShouldBeSuccess();
            result.Value!.AssertOrderDto(command);
        }

        [Test]
        public async Task Should_fail_when_items_is_empty()
        {
            var command = new CreateOrderCommand(
                Guid.NewGuid().ToString(),
                new List<CreateOrderItemCommand>());

            var result = await Mediator.Send(command);

            result.ShouldBeFailure(typeof(InvalidOperationException));
        }
    }
}
