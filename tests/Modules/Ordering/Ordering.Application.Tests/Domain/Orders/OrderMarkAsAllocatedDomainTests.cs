using FluentAssertions;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Tests.Domain.Orders;

[TestFixture]
public class OrderMarkAsAllocatedDomainTests
{
    [Test]
    public void MarkAsAllocated_when_processing_sets_order_allocated_true()
    {
        var order = new Order("N-MA1", "cust");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });
        order.Accept();

        order.MarkAsAllocated();

        order.OrderAllocated.Should().BeTrue();
    }

    [Test]
    public void MarkAsAllocated_throws_when_not_processing()
    {
        var order = new Order("N-MA2", "cust");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });

        var act = () => order.MarkAsAllocated();

        act.Should().Throw<InvalidOperationException>();
    }
}
