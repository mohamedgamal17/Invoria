using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Application.OrderAllocationConsumptions.Handlers;
using Invoria.Ordering.Application.ReportOrderSalesProfitMetrics.Commands.RecordOrderSalesProfitMetrics;
using Invoria.Ordering.Domain.OrderAllocationConsumptions;
using Invoria.Ordering.Domain.OrderAllocationConsumptions.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Invoria.Ordering.Application.Tests.OrderAllocationConsumptions.Handlers;

[TestFixture]
public class OrderAllocationConsumptionCreatedDomainEventHandlerTests
{
    [Test]
    public async Task Handle_sends_record_order_sales_profit_command()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(
                It.IsAny<RecordOrderSalesProfitMetricsCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Empty.Value));

        var handler = new OrderAllocationConsumptionCreatedDomainEventHandler(
            BuildServiceScopeFactory(mediator.Object),
            Mock.Of<ILogger<OrderAllocationConsumptionCreatedDomainEventHandler>>());

        var consumption = BuildConsumption();
        var domainEvent = new OrderAllocationConsumptionCreatedDomainEvent(consumption);

        await handler.Handle(domainEvent, CancellationToken.None);

        mediator.Verify(
            m => m.Send(
                It.Is<RecordOrderSalesProfitMetricsCommand>(c =>
                    c.OrderId == "order-1" &&
                    c.AllocationId == "alloc-1" &&
                    c.OccurredOn == domainEvent.OccurredOn),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_throws_when_command_fails()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(
                It.IsAny<RecordOrderSalesProfitMetricsCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Empty>(new InvalidOperationException("boom")));

        var handler = new OrderAllocationConsumptionCreatedDomainEventHandler(
            BuildServiceScopeFactory(mediator.Object),
            Mock.Of<ILogger<OrderAllocationConsumptionCreatedDomainEventHandler>>());

        var domainEvent = new OrderAllocationConsumptionCreatedDomainEvent(BuildConsumption());

        var act = async () => await handler.Handle(domainEvent, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private static IServiceScopeFactory BuildServiceScopeFactory(IMediator mediator)
    {
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider
            .Setup(sp => sp.GetService(typeof(IMediator)))
            .Returns(mediator);

        var scope = new Mock<IServiceScope>();
        scope
            .Setup(s => s.ServiceProvider)
            .Returns(serviceProvider.Object);

        var serviceScopeFactory = new Mock<IServiceScopeFactory>();
        serviceScopeFactory
            .Setup(f => f.CreateScope())
            .Returns(scope.Object);

        return serviceScopeFactory.Object;
    }

    private static OrderAllocationConsumption BuildConsumption()
    {
        var line = new OrderAllocationConsumptionLine(
            Guid.NewGuid().ToString("N"),
            "item-1",
            "product-1",
            5);
        line.AddBatchAllocation(new OrderAllocationConsumptionBatch(
            Guid.NewGuid().ToString("N"),
            "batch-1",
            5,
            8m));

        return OrderAllocationConsumption.Create("order-1", "alloc-1", [line]);
    }
}
