using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Domain.Orders.Events;

namespace Invoria.Ordering.Application.Tests.Domain.Orders;

[TestFixture]
public class OrderDispatchDomainTests
{
    [Test]
    public void MarkDispatched_sets_fulfillment_to_dispatched_and_raises_domain_event_when_allocated()
    {
        var order = new Order("N-D1", "cust");
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(order, "order-dispatch-1");
        order.UpdateItems(new List<OrderItem> { new("p1", 2, 10m) });
        order.Accept();
        order.MarkInventoryAllocated();
        order.ClearDomainEvents();

        var item = order.Items[0];
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(item, "line-1");

        order.MarkDispatched();

        order.Status.Should().Be(OrderStatus.Accepted);
        order.FullfillmentStatus.Should().Be(FullfillmentStatus.Dispatched);
        order.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<OrderDispatchedDomainEvent>();
        var ev = (OrderDispatchedDomainEvent)order.DomainEvents.Single();
        ev.OrderId.Should().Be("order-dispatch-1");
        ev.OrderNumber.Should().Be("N-D1");
        ev.CustomerId.Should().Be("cust");
        ev.Lines.Should().ContainSingle();
        ev.Lines[0].OrderItemId.Should().Be("line-1");
        ev.Lines[0].ProductId.Should().Be("p1");
        ev.Lines[0].Quantity.Should().Be(2);
    }

    [Test]
    public void MarkDispatched_is_idempotent_when_already_dispatched()
    {
        var order = new Order("N-D2", "cust");
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(order, "order-dispatch-2");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });
        order.Accept();
        order.MarkInventoryAllocated();
        order.MarkDispatched();
        order.ClearDomainEvents();

        var act = () => order.MarkDispatched();

        act.Should().NotThrow();
        order.FullfillmentStatus.Should().Be(FullfillmentStatus.Dispatched);
        order.DomainEvents.Should().BeEmpty();
    }

    [Test]
    public void MarkDispatched_throws_when_not_allocated()
    {
        var order = new Order("N-D3", "cust");
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(order, "order-not-alloc");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });
        order.Accept();

        var act = () => order.MarkDispatched();

        act.Should().Throw<InvalidOperationException>();
    }
}
