using FluentAssertions;
using Invoria.Ordering.Domain.OrderAllocationConsumptions;
using Invoria.Ordering.Domain.OrderAllocationConsumptions.Events;

namespace Invoria.Ordering.Application.Tests.Domain.OrderAllocationConsumptions;

[TestFixture]
public class OrderAllocationConsumptionCreatedDomainTests
{
    [Test]
    public void Create_raises_OrderAllocationConsumptionCreatedDomainEvent_with_aggregate()
    {
        var consumption = BuildConsumption();

        consumption.Id.Should().NotBeNullOrWhiteSpace();
        consumption.OrderId.Should().Be("order-1");
        consumption.AllocationId.Should().Be("alloc-1");

        consumption.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderAllocationConsumptionCreatedDomainEvent>();

        var domainEvent = (OrderAllocationConsumptionCreatedDomainEvent)consumption.DomainEvents.Single();
        domainEvent.OrderId.Should().Be("order-1");
        domainEvent.AllocationId.Should().Be("alloc-1");
        domainEvent.Consumption.Should().BeSameAs(consumption);
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
