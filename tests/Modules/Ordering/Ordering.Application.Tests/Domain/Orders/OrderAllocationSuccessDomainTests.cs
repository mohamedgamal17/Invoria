using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Contracts.Orders;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Tests.Domain.Orders;

[TestFixture]
public class OrderAllocationSuccessDomainTests
{
    [Test]
    public void MarkInventoryAllocated_is_noop_when_accepted()
    {
        var order = new Order("N-1", "cust");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });
        order.Accept();

        order.MarkInventoryAllocated();

        order.Status.Should().Be(OrderStatus.Accepted);
    }

    [Test]
    public void MarkInventoryAllocated_is_idempotent_when_already_accepted()
    {
        var order = new Order("N-2", "cust");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });
        order.Accept();

        var act = () => order.MarkInventoryAllocated();

        act.Should().NotThrow();
        order.Status.Should().Be(OrderStatus.Accepted);
    }

    [Test]
    public void MarkInventoryAllocated_throws_when_not_accepted()
    {
        var order = new Order("N-3", "cust");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });

        var act = () => order.MarkInventoryAllocated();

        act.Should().Throw<InvalidOperationException>();
    }
}
