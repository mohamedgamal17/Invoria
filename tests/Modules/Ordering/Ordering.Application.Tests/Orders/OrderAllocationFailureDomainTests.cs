using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Tests.Orders;

[TestFixture]
public class OrderAllocationFailureDomainTests
{
    [Test]
    public void CancelDueToAllocationFailure_sets_cancelled_and_resets_fulfillment()
    {
        var order = new Order("N-1", "cust");
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(order, "order-alloc-fail-1");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });
        order.Accept();

        order.CancelDueToAllocationFailure("Insufficient stock for product p1.");

        order.Status.Should().Be(OrderStatus.Cancelled);
        order.FullfillmentStatus.Should().Be(FullfillmentStatus.Pending);
    }

    [Test]
    public void CancelDueToAllocationFailure_throws_when_order_not_accepted_allocating()
    {
        var order = new Order("N-2", "cust");
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(order, "order-pending");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });

        var act = () => order.CancelDueToAllocationFailure("reason");

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void CancelDueToAllocationFailure_throws_when_reason_empty()
    {
        var order = new Order("N-3", "cust");
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(order, "order-3");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });
        order.Accept();

        var act = () => order.CancelDueToAllocationFailure(" ");

        act.Should().Throw<ArgumentException>();
    }
}
