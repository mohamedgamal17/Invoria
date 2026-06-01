using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Contracts.Orders;
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
    public void Refuse_when_accepted_sets_refused_and_raises_release_and_refused_events()
    {
        var order = CreateOrderWithItems("ref-alloc");
        order.Accept();
        order.ClearDomainEvents();

        order.Refuse();

        order.Status.Should().Be(OrderStatus.Refused);
        order.DomainEvents.Should().HaveCount(2);
        order.DomainEvents.Should().ContainSingle(e => e is OrderRefusalReleaseRequestedDomainEvent);
        order.DomainEvents.Should().ContainSingle(e => e is OrderRefusedDomainEvent);
    }

    [Test]
    public void Refuse_when_completed_sets_refused_and_refused_event()
    {
        var order = CreateOrderWithItems("ref-done");
        order.Accept();
        order.MarkInventoryAllocated();
        order.MarkDispatched();
        order.MarkShipped();
        order.Complete();
        order.ClearDomainEvents();

        order.Refuse();

        order.Status.Should().Be(OrderStatus.Refused);
        order.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<OrderRefusedDomainEvent>();
    }

    [Test]
    public void Refuse_throws_when_pending()
    {
        var order = CreateOrderWithItems("ref-pending");

        var act = () => order.Refuse();

        act.Should().Throw<InvalidOperationException>();
    }
}
