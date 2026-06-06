using FluentAssertions;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Tests.Domain.Orders;

[TestFixture]
public class OrderRequestRevisionDomainTests
{
    private static Order CreateProcessingOrder()
    {
        var order = new Order("N-RR1", "cust");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });
        order.Accept();
        return order;
    }

    [Test]
    public void RequestRevision_when_processing_and_allocated_sets_revision_pending_and_clears_allocated()
    {
        var order = CreateProcessingOrder();
        order.RecordAllocation("alloc-1");
        order.MarkAsAllocated();

        order.RequestRevision();

        order.Status.Should().Be(OrderStatus.RevisionPending);
        order.OrderAllocated.Should().BeFalse();
        order.AllocationId.Should().Be("alloc-1");
    }

    [Test]
    public void RequestRevision_throws_when_not_processing()
    {
        var order = new Order("N-RR2", "cust");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });

        var act = () => order.RequestRevision();

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void RequestRevision_throws_when_processing_but_not_allocated()
    {
        var order = CreateProcessingOrder();

        var act = () => order.RequestRevision();

        act.Should().Throw<InvalidOperationException>();
    }
}
