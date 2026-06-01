using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Contracts.Orders;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Domain.Orders.Events;

namespace Invoria.Ordering.Application.Tests.Domain.Orders;

[TestFixture]
public class OrderDispatchDomainTests
{
    [Test]
    public void MarkDispatched_raises_domain_event_when_accepted()
    {
        var order = new Order("N-D1", "cust");
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(order, "order-dispatch-1");
        order.UpdateItems(new List<OrderItem> { new("p1", 2, 10m) });
        order.Accept();
        order.MarkInventoryAllocated();
        order.ClearDomainEvents();

        order.MarkDispatched();

        order.Status.Should().Be(OrderStatus.Accepted);
        order.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<OrderDispatchedDomainEvent>();
    }

    [Test]
    public void MarkDispatched_is_idempotent_when_already_dispatched()
    {
        var order = new Order("N-D2", "cust");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });
        order.Accept();
        order.MarkInventoryAllocated();
        order.MarkDispatched();
        order.ClearDomainEvents();

        var act = () => order.MarkDispatched();

        act.Should().NotThrow();
        order.DomainEvents.Should().BeEmpty();
    }

    [Test]
    public void MarkDispatched_throws_when_not_accepted()
    {
        var order = new Order("N-D3", "cust");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });

        var act = () => order.MarkDispatched();

        act.Should().Throw<InvalidOperationException>();
    }
}
