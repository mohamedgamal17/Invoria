using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Tests.Domain.Orders;

[TestFixture]
public class OrderAllocationSuccessDomainTests
{
    [Test]
    public void MarkInventoryAllocated_sets_fulfillment_to_allocated_when_accepted_and_allocating()
    {
        var order = new Order("N-1", "cust");
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(order, "order-alloc-ok-1");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });
        order.Accept();

        order.MarkInventoryAllocated();

        order.Status.Should().Be(OrderStatus.Accepted);
        order.FullfillmentStatus.Should().Be(FullfillmentStatus.Allocated);
    }

    [Test]
    public void MarkInventoryAllocated_is_idempotent_when_already_allocated()
    {
        var order = new Order("N-2", "cust");
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(order, "order-alloc-ok-2");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });
        order.Accept();
        order.MarkInventoryAllocated();

        var act = () => order.MarkInventoryAllocated();

        act.Should().NotThrow();
        order.FullfillmentStatus.Should().Be(FullfillmentStatus.Allocated);
    }

    [Test]
    public void MarkInventoryAllocated_throws_when_not_accepted_or_not_allocating()
    {
        var order = new Order("N-3", "cust");
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(order, "order-pending");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });

        var act = () => order.MarkInventoryAllocated();

        act.Should().Throw<InvalidOperationException>();
    }
}
