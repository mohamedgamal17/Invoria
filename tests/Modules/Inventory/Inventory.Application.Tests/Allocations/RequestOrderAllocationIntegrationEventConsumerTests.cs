using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Application.Allocations.Consumers;
using Invoria.Inventory.Application.Allocations.Queries.GetOrderAllocationConsumption;
using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.Ordering.Contracts.Orders.Events;
using MediatR;
using Moq;
using Rebus.Bus;

namespace Invoria.Inventory.Application.Tests.Allocations;

[TestFixture]
public class RequestOrderAllocationIntegrationEventConsumerTests
{
    [Test]
    public async Task Handle_publishes_order_allocation_consumption_when_query_succeeds()
    {
        var mediator = new Mock<IMediator>();
        var consumption = new OrderAllocationConsumptionIntegrationEvent
        {
            OrderId = "order-1",
            AllocationId = "alloc-1",
            Lines = []
        };
        mediator
            .Setup(m => m.Send(
                It.IsAny<GetOrderAllocationConsumptionQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(consumption));

        var bus = new Mock<IBus>();
        bus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        var consumer = new RequestOrderAllocationIntegrationEventConsumer(
            mediator.Object,
            bus.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<RequestOrderAllocationIntegrationEventConsumer>>());

        var message = new RequestOrderAllocationIntegrationEvent
        {
            OrderId = "order-1",
            AllocationId = "alloc-1"
        };

        await consumer.Handle(message);

        mediator.Verify(
            m => m.Send(
                It.Is<GetOrderAllocationConsumptionQuery>(q => q.AllocationId == "alloc-1"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        bus.Verify(
            b => b.Publish(
                It.Is<OrderAllocationConsumptionIntegrationEvent>(e =>
                    e.OrderId == "order-1" &&
                    e.AllocationId == "alloc-1"),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_throws_when_query_fails()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(
                It.IsAny<GetOrderAllocationConsumptionQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<OrderAllocationConsumptionIntegrationEvent>(
                new InvalidOperationException("boom")));

        var bus = new Mock<IBus>();

        var consumer = new RequestOrderAllocationIntegrationEventConsumer(
            mediator.Object,
            bus.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<RequestOrderAllocationIntegrationEventConsumer>>());

        var message = new RequestOrderAllocationIntegrationEvent
        {
            OrderId = "order-1",
            AllocationId = "alloc-1"
        };

        var act = async () => await consumer.Handle(message);

        await act.Should().ThrowAsync<InvalidOperationException>();

        bus.Verify(
            b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()),
            Times.Never);
    }
}
