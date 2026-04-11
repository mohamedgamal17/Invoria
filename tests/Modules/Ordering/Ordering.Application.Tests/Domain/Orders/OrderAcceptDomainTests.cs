using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
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
}
