using FluentAssertions;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Tests.Domain.Orders;

[TestFixture]
public class OrderStateTransitionHistoryDomainTests
{
    [Test]
    public void State_transition_methods_append_history_with_before_after_states()
    {
        var order = new Order("N-H1", "cust");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });

        order.Accept();
        order.MarkInventoryAllocated();
        order.MarkDispatched();
        order.MarkShipped();
        order.Complete();

        order.StateTransitionHistory.Should().HaveCount(5);
        var history = order.StateTransitionHistory.ToList();

        var accepted = history[0];
        accepted.FromStatus.Should().Be(OrderStatus.Pending);
        accepted.ToStatus.Should().Be(OrderStatus.Accepted);
        accepted.FromFullfillmentStatus.Should().Be(FullfillmentStatus.Pending);
        accepted.ToFullfillmentStatus.Should().Be(FullfillmentStatus.Allocating);
        accepted.Reason.Should().BeNull();

        var allocated = history[1];
        allocated.FromStatus.Should().Be(OrderStatus.Accepted);
        allocated.ToStatus.Should().Be(OrderStatus.Accepted);
        allocated.FromFullfillmentStatus.Should().Be(FullfillmentStatus.Allocating);
        allocated.ToFullfillmentStatus.Should().Be(FullfillmentStatus.Allocated);

        var dispatched = history[2];
        dispatched.FromFullfillmentStatus.Should().Be(FullfillmentStatus.Allocated);
        dispatched.ToFullfillmentStatus.Should().Be(FullfillmentStatus.Dispatched);

        var shipped = history[3];
        shipped.FromStatus.Should().Be(OrderStatus.Accepted);
        shipped.ToStatus.Should().Be(OrderStatus.Shipped);
        shipped.FromFullfillmentStatus.Should().Be(FullfillmentStatus.Dispatched);
        shipped.ToFullfillmentStatus.Should().Be(FullfillmentStatus.Dispatched);

        var completed = history[4];
        completed.FromStatus.Should().Be(OrderStatus.Shipped);
        completed.ToStatus.Should().Be(OrderStatus.Completed);
        completed.FromFullfillmentStatus.Should().Be(FullfillmentStatus.Dispatched);
        completed.ToFullfillmentStatus.Should().Be(FullfillmentStatus.Dispatched);
    }
}
