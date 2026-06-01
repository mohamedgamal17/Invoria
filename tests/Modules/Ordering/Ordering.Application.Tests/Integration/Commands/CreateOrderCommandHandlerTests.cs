using Invoria.Application.Tests.Extensions;
using Invoria.Ordering.Application.Orders.Commands.CreateOrder;
using Invoria.Ordering.Application.Tests.Assertions;
using Invoria.Ordering.Contracts.Events;
using Invoria.Ordering.Contracts.Orders;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Tests.Integration.Commands
{
    [TestFixture]
    public class CreateOrderCommandHandlerTests : OrderTestFixture
    {
        [SetUp]
        public void ResetBusMock()
        {
            var busMock = ServiceProvider.GetRequiredService<Mock<IBus>>();
            busMock.Reset();
            busMock
                .Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
                .Returns(Task.CompletedTask);
        }

        [TestCase(OrderPaymentType.Immediate)]
        [TestCase(OrderPaymentType.Debt)]
        public async Task Should_create_order_and_return_order_dto(OrderPaymentType paymentType)
        {
            var command = new CreateOrderCommand(
                Guid.NewGuid().ToString(),
                new List<CreateOrderItemCommand>
                {
                    new(Guid.NewGuid().ToString(), 2, 10m),
                    new(Guid.NewGuid().ToString(), 1, 25m)
                },
                paymentType);

            var result = await Mediator.Send(command);

            result.ShouldBeSuccess();
            result.Value!.AssertOrderDto(command);
        }

        [TestCase(OrderPaymentType.Immediate)]
        [TestCase(OrderPaymentType.Debt)]
        public async Task Should_publish_OrderCreatedIntegration_event_once(OrderPaymentType paymentType)
        {
            var busMock = ServiceProvider.GetRequiredService<Mock<IBus>>();

            var command = new CreateOrderCommand(
                Guid.NewGuid().ToString(),
                new List<CreateOrderItemCommand>
                {
                    new(Guid.NewGuid().ToString(), 2, 10m),
                    new(Guid.NewGuid().ToString(), 1, 25m)
                },
                paymentType);

            var result = await Mediator.Send(command);

            result.ShouldBeSuccess();
            var expectedTotal = result.Value!.Items.Sum(i => i.Price * i.Quantity);
            busMock.Verify(
                b => b.Publish(
                    It.Is<OrderCreatedIntegrationEvent>(e =>
                        e.Order.Id == result.Value.Id &&
                        e.Order.OrderNumber == result.Value.OrderNumber &&
                        e.Order.CustomerId == result.Value.CustomerId &&
                        e.Order.OrderStatus == result.Value.Status &&
                        e.Order.PaymentType == result.Value.PaymentType &&
                        e.Order.PaymentStatus == result.Value.PaymentStatus &&
                        e.Order.TotalOrderAmount == expectedTotal &&
                        e.Order.AmountPaid == result.Value.AmountPaid &&
                        e.Order.AmountOutstanding == result.Value.AmountOutstanding &&
                        e.Order.Lines.Count == result.Value.Items.Count),
                    It.IsAny<Dictionary<string, string>>()),
                Times.Once);
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
