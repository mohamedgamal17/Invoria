using Invoria.Inventory.Application.Batches.Consumers;
using Invoria.Inventory.Application.Batches.Commands.AllocateOrder;
using Invoria.Inventory.Domain.Batches;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Contracts.Events;
using Invoria.Ordering.Contracts.Models;
using MediatR;
using Moq;
using Rebus.Bus;

namespace Invoria.Inventory.Application.Tests.Batches;

[TestFixture]
public class AllocateOrderIntegrationConsumerTests
{
    [Test]
    public async Task Publishes_failure_event_when_allocation_fails()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<AllocateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Empty>(new InvalidOperationException("Insufficient stock for product x.")));

        var bus = new Mock<IBus>();
        var consumer = new AllocateOrderIntegrationEventConsumer(mediator.Object, bus.Object);

        var message = new AllocateOrderIntegrationEvent
        {
            Id = "order-1",
            OrderNumber = "ON-9",
            CustomerId = "c1",
            Items = new List<OrderItemModel> { new() { Id = "i1", ProductId = "p1", Quantity = 1 } }
        };

        await consumer.Handle(message);

        bus.Verify(
            b => b.Publish(
                It.Is<OrderAllocationFailedIntegrationEvent>(e =>
                    e.OrderId == "order-1" &&
                    e.OrderNumber == "ON-9" &&
                    e.Reason == "Insufficient stock for product x." &&
                    e.Details != null &&
                    e.Details.Contains("Insufficient stock for product x.") &&
                    e.ItemErrors.Count == 0),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Publishes_typed_item_errors_when_pre_flight_fails()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<AllocateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Empty>(new OrderAllocationPreFlightException(
            [
                new OrderAllocationPreFlightError("p1", 5, 2)
            ])));

        var bus = new Mock<IBus>();
        var consumer = new AllocateOrderIntegrationEventConsumer(mediator.Object, bus.Object);

        var message = new AllocateOrderIntegrationEvent
        {
            Id = "order-typed-1",
            OrderNumber = "ON-typed-1",
            CustomerId = "c1",
            Items = new List<OrderItemModel> { new() { Id = "i1", ProductId = "p1", Quantity = 5 } }
        };

        await consumer.Handle(message);

        bus.Verify(
            b => b.Publish(
                It.Is<OrderAllocationFailedIntegrationEvent>(e =>
                    e.OrderId == "order-typed-1" &&
                    e.ItemErrors.Count == 1 &&
                    e.ItemErrors[0].OrderItemId == "i1" &&
                    e.ItemErrors[0].ProductId == "p1" &&
                    e.ItemErrors[0].RequestedQuantity == 5 &&
                    e.ItemErrors[0].AvailableQuantity == 2 &&
                    e.ItemErrors[0].Message.Contains("Insufficient stock")),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Publishes_success_event_when_allocation_succeeds()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<AllocateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Empty.Value));

        var bus = new Mock<IBus>();
        var consumer = new AllocateOrderIntegrationEventConsumer(mediator.Object, bus.Object);

        var message = new AllocateOrderIntegrationEvent
        {
            Id = "order-2",
            OrderNumber = "ON-8",
            CustomerId = "c1",
            Items = new List<OrderItemModel> { new() { Id = "i1", ProductId = "p1", Quantity = 2 } }
        };

        await consumer.Handle(message);

        bus.Verify(
            b => b.Publish(
                It.Is<OrderAllocationSucceededIntegrationEvent>(e =>
                    e.OrderId == "order-2" &&
                    e.OrderNumber == "ON-8" &&
                    e.CustomerId == "c1" &&
                    e.AllocatedAt != default &&
                    e.AllocatedLines.Count == 1 &&
                    e.AllocatedLines[0].OrderItemId == "i1" &&
                    e.AllocatedLines[0].ProductId == "p1" &&
                    e.AllocatedLines[0].Quantity == 2),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);

        bus.Verify(
            b => b.Publish(It.IsAny<OrderAllocationFailedIntegrationEvent>(), It.IsAny<Dictionary<string, string>>()),
            Times.Never);
    }
}

