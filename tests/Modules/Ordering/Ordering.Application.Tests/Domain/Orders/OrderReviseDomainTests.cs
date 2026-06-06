using FluentAssertions;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Tests.Domain.Orders;

[TestFixture]
public class OrderReviseDomainTests
{
    private static Order CreateProcessingOrder()
    {
        var order = new Order("N-R9", "cust");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });
        order.Accept();
        return order;
    }

    [Test]
    public void Revise_when_processing_sets_revision_and_preserves_allocation()
    {
        var order = CreateProcessingOrder();
        order.RecordAllocation("alloc-1");

        order.Revise();

        order.Status.Should().Be(OrderStatus.Revision);
        order.AllocationId.Should().Be("alloc-1");
    }

    [Test]
    public void Revise_from_revision_pending_sets_revision()
    {
        var order = CreateProcessingOrder();
        order.RecordAllocation("alloc-1");
        order.MarkAsAllocated();
        order.RequestRevision();

        order.Revise();

        order.Status.Should().Be(OrderStatus.Revision);
        order.AllocationId.Should().Be("alloc-1");
    }

    [Test]
    public void Revise_throws_when_not_processing_or_revision_pending()
    {
        var order = new Order("N-R8", "cust");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });

        var act = () => order.Revise();

        act.Should().Throw<InvalidOperationException>();
    }
}
