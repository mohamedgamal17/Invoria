using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.Inventory.Contracts.Allocations.Models;
using Invoria.Ordering.Application.OrderAllocationConsumptions.Commands.CreateOrderAllocationConsumption;
using Invoria.Ordering.Application.OrderAllocationConsumptions.Consumers;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Invoria.Ordering.Application.Tests.OrderAllocationConsumptions.Consumers;

[TestFixture]
public class OrderAllocationConsumptionIntegrationEventConsumerTests
{
    [Test]
    public async Task Handle_creates_order_allocation_consumption_when_command_succeeds()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(
                It.IsAny<CreateOrderAllocationConsumptionCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Empty.Value));

        var consumer = new OrderAllocationConsumptionIntegrationEventConsumer(
            mediator.Object,
            Mock.Of<ILogger<OrderAllocationConsumptionIntegrationEventConsumer>>());

        var message = new OrderAllocationConsumptionIntegrationEvent
        {
            OrderId = "order-1",
            AllocationId = "alloc-1",
            Lines =
            [
                new OrderAllocationConsumptionLineModel
                {
                    OrderItemId = "item-1",
                    ProductId = "product-1",
                    QuantityRequested = 5,
                    BatchAllocations =
                    [
                        new OrderAllocationConsumptionBatchModel
                        {
                            BatchId = "batch-1",
                            Quantity = 3,
                            UnitPrice = 10m
                        }
                    ]
                }
            ]
        };

        await consumer.Handle(message);

        mediator.Verify(
            m => m.Send(
                It.Is<CreateOrderAllocationConsumptionCommand>(c =>
                    c.OrderId == "order-1" &&
                    c.AllocationId == "alloc-1" &&
                    c.Lines.Count == 1 &&
                    c.Lines[0].OrderItemId == "item-1" &&
                    c.Lines[0].BatchAllocations[0].BatchId == "batch-1" &&
                    c.Lines[0].BatchAllocations[0].UnitPrice == 10m),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_throws_when_command_fails()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(
                It.IsAny<CreateOrderAllocationConsumptionCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Empty>(new InvalidOperationException("boom")));

        var consumer = new OrderAllocationConsumptionIntegrationEventConsumer(
            mediator.Object,
            Mock.Of<ILogger<OrderAllocationConsumptionIntegrationEventConsumer>>());

        var message = new OrderAllocationConsumptionIntegrationEvent
        {
            OrderId = "order-1",
            AllocationId = "alloc-1",
            Lines = []
        };

        var act = async () => await consumer.Handle(message);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
