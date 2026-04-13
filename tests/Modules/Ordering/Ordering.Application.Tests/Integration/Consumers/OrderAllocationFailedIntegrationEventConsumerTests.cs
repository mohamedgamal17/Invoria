using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Application.Orders.Commands.RecordOrderAllocationFailed;
using Invoria.Ordering.Application.Orders.Consumers;
using Invoria.Ordering.Contracts.Events;
using MediatR;
using Moq;

namespace Invoria.Ordering.Application.Tests.Integration.Consumers;

[TestFixture]
public class OrderAllocationFailedIntegrationEventConsumerTests
{
    [Test]
    public async Task Sends_record_allocation_failed_command_from_event_payload()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<RecordOrderAllocationFailedCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Empty.Value));

        var consumer = new OrderAllocationFailedIntegrationEventConsumer(
            mediator.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<OrderAllocationFailedIntegrationEventConsumer>>());

        var message = new OrderAllocationFailedIntegrationEvent
        {
            OrderId = "order-failed-1",
            OrderNumber = "ON-404",
            CustomerId = "cust-1",
            Reason = "Insufficient stock",
            Details = "p1 not enough",
            ItemErrors =
            [
                new OrderAllocationItemErrorModel
                {
                    OrderItemId = "line-1",
                    ProductId = "p1",
                    RequestedQuantity = 6,
                    AvailableQuantity = 2,
                    Shortage = 4
                }
            ]
        };

        await consumer.Handle(message);

        mediator.Verify(
            m => m.Send(
                It.Is<RecordOrderAllocationFailedCommand>(c =>
                    c.OrderId == "order-failed-1" &&
                    c.OrderNumber == "ON-404" &&
                    c.CustomerId == "cust-1" &&
                    c.Reason == "Insufficient stock" &&
                    c.ItemErrors.Count == 1 &&
                    c.ItemErrors[0].ItemId == "p1" &&
                    c.ItemErrors[0].QuantityRequested == 6 &&
                    c.ItemErrors[0].QuantityAvailable == 2 &&
                    c.ItemErrors[0].Shortage == 4),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
