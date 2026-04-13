using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Contracts.Orders;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Domain.Orders.Events;

namespace Invoria.Ordering.Application.Tests.Domain.Orders;

[TestFixture]
public class OrderAcceptDomainTests
{
    private static void SetEntityId(Entity<string> entity, string id)
    {
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(entity, id);
    }

    [Test]
    public void Accept_raises_OrderAcceptedDomainEvent_with_lines_and_sets_allocating()
    {
        var order = new Order("TEST-1", Guid.NewGuid().ToString());
        order.UpdateItems(
        [
            new OrderItem("p1", 2, 10m)
        ]);
        SetEntityId(order, "order-accept-1");
        var item = order.Items[0];
        SetEntityId(item, "line-accept-1");

        order.Accept();

        order.FullfillmentStatus.Should().Be(FullfillmentStatus.Allocating);
        order.Status.Should().Be(OrderStatus.Accepted);
        order.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<OrderAcceptedDomainEvent>();
        var ev = (OrderAcceptedDomainEvent)order.DomainEvents.Single();
        ev.OrderId.Should().Be("order-accept-1");
        ev.OrderNumber.Should().Be("TEST-1");
        ev.CustomerId.Should().Be(order.CustomerId);
        ev.Lines.Should().ContainSingle();
        ev.Lines[0].OrderItemId.Should().Be("line-accept-1");
        ev.Lines[0].ProductId.Should().Be("p1");
        ev.Lines[0].Quantity.Should().Be(2);
    }

    [Test]
    public void Accept_maps_all_order_lines()
    {
        var order = new Order("N-A2", "cust-2");
        SetEntityId(order, "order-accept-2");
        order.UpdateItems(
        [
            new OrderItem("pa", 1, 1m),
            new OrderItem("pb", 3, 2m)
        ]);
        SetEntityId(order.Items[0], "la");
        SetEntityId(order.Items[1], "lb");

        order.Accept();

        var ev = order.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<OrderAcceptedDomainEvent>().Subject;
        ev.Lines.Should().HaveCount(2);
        ev.Lines[0].Should().Be(new OrderAcceptedLine("la", "pa", 1));
        ev.Lines[1].Should().Be(new OrderAcceptedLine("lb", "pb", 3));
    }

    [Test]
    public void Accept_when_reopened_and_fulfillment_on_hold_sets_accepted_and_allocating()
    {
        var order = new Order("N-A3", "cust-3");
        SetEntityId(order, "order-accept-reopen");
        order.UpdateItems([new OrderItem("p1", 2, 10m)]);
        var item = order.Items[0];
        SetEntityId(item, "line-accept-reopen");

        order.Accept();
        order.FullfillmentStatus = FullfillmentStatus.Pending;
        order.Reopen();
        order.ClearDomainEvents();

        order.Accept();

        order.FullfillmentStatus.Should().Be(FullfillmentStatus.Allocating);
        order.Status.Should().Be(OrderStatus.Accepted);
        order.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<OrderAcceptedDomainEvent>();
        var ev = (OrderAcceptedDomainEvent)order.DomainEvents.Single();
        ev.OrderId.Should().Be("order-accept-reopen");
        ev.OrderNumber.Should().Be("N-A3");
        ev.CustomerId.Should().Be("cust-3");
        ev.Lines.Should().ContainSingle();
        ev.Lines[0].Should().Be(new OrderAcceptedLine("line-accept-reopen", "p1", 2));
    }

    [Test]
    public void Accept_throws_when_fulfillment_is_not_pending_or_on_hold()
    {
        var order = new Order("N-A4", "cust-4");
        SetEntityId(order, "order-accept-bad-fulfillment");
        order.UpdateItems([new OrderItem("p1", 1, 1m)]);
        order.FullfillmentStatus = FullfillmentStatus.Allocated;

        var act = () => order.Accept();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Pending or On Hold*");
    }
}
