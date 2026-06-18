using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Tests.Domain.Orders;

[TestFixture]
public class OrderRecordAllocationDomainTests
{
    private static void SetEntityId(Entity<string> entity, string id)
    {
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(entity, id);
    }

    [Test]
    public void RecordAllocation_sets_allocation_id_when_processing()
    {
        var order = new Order("TEST-1", Guid.NewGuid().ToString());
        order.UpdateItems([new OrderItem("p1", 2, 10m)]);
        SetEntityId(order, "order-1");
        order.Accept();

        order.RecordAllocation("alloc-1");

        order.AllocationId.Should().Be("alloc-1");
        order.Status.Should().Be(OrderStatus.Processing);
    }

    [Test]
    public void RecordAllocation_throws_when_not_processing()
    {
        var order = new Order("TEST-2", Guid.NewGuid().ToString());
        order.UpdateItems([new OrderItem("p1", 1, 10m)]);

        var act = () => order.RecordAllocation("alloc-1");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Processing*");
        order.AllocationId.Should().BeNull();
    }
}
