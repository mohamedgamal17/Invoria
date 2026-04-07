using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Domain.Orders.Events;

namespace Invoria.Ordering.Application.Tests.Domain.Orders;

[TestFixture]
public class OrderRefusalDomainTests
{
    private static Order CreateOrderWithItems(string id)
    {
        var order = new Order("RF-1", "cust");
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(order, id);
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });
        return order;
    }

    [Test]
    public void Refuse_when_allocated_sets_refused_releasing_and_release_event()
    {
        var order = CreateOrderWithItems("ref-alloc");
        order.Accept();
        order.MarkInventoryAllocated();
        order.ClearDomainEvents();

        order.Refuse();

        order.Status.Should().Be(OrderStatus.Refused);
        order.FullfillmentStatus.Should().Be(FullfillmentStatus.Releasing);
        order.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<OrderRefusalReleaseRequestedDomainEvent>();
    }

    [Test]
    public void Refuse_when_allocating_sets_refused_cancelled_and_refused_event()
    {
        var order = CreateOrderWithItems("ref-allocating");
        order.Accept();
        order.ClearDomainEvents();

        order.Refuse();

        order.Status.Should().Be(OrderStatus.Refused);
        order.FullfillmentStatus.Should().Be(FullfillmentStatus.Cancelled);
        order.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<OrderRefusedDomainEvent>();
    }

    [Test]
    public void Refuse_when_accepted_and_fulfillment_pending_sets_refused_cancelled_and_refused_event()
    {
        var order = CreateOrderWithItems("ref-pending");
        typeof(Order).GetProperty(nameof(Order.Status))!.SetValue(order, OrderStatus.Accepted);

        order.Refuse();

        order.Status.Should().Be(OrderStatus.Refused);
        order.FullfillmentStatus.Should().Be(FullfillmentStatus.Cancelled);
        order.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<OrderRefusedDomainEvent>();
    }

    [Test]
    public void Refuse_when_releasing_throws()
    {
        var order = CreateOrderWithItems("ref-rel");
        order.Accept();
        order.MarkInventoryAllocated();
        order.Reopen();

        var act = () => order.Refuse();

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void CompleteRefusalAfterInventoryReleased_sets_cancelled()
    {
        var order = CreateOrderWithItems("ref-complete");
        order.Accept();
        order.MarkInventoryAllocated();
        order.Refuse();
        order.ClearDomainEvents();

        order.CompleteRefusalAfterInventoryReleased();

        order.Status.Should().Be(OrderStatus.Refused);
        order.FullfillmentStatus.Should().Be(FullfillmentStatus.Cancelled);
    }

    [Test]
    public void Refuse_when_completed_sets_refused_and_refused_event()
    {
        var order = CreateOrderWithItems("ref-done");
        order.Accept();
        order.MarkInventoryAllocated();
        order.MarkDispatched();
        order.Complete();
        order.ClearDomainEvents();

        order.Refuse();

        order.Status.Should().Be(OrderStatus.Refused);
        order.FullfillmentStatus.Should().Be(FullfillmentStatus.Dispatched);
        order.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<OrderRefusedDomainEvent>();
    }

    [Test]
    public void Refuse_when_dispatched_sets_refused_and_refused_event()
    {
        var order = CreateOrderWithItems("ref-disp");
        order.Accept();
        order.MarkInventoryAllocated();
        order.MarkDispatched();
        order.ClearDomainEvents();

        order.Refuse();

        order.Status.Should().Be(OrderStatus.Refused);
        order.FullfillmentStatus.Should().Be(FullfillmentStatus.Dispatched);
        order.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<OrderRefusedDomainEvent>();
    }
}
