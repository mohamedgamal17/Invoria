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
    public void Accept_raises_OrderAcceptedDomainEvent_with_lines_and_sets_accepted()
    {
        var order = new Order("TEST-1", Guid.NewGuid().ToString());
        order.UpdateItems([new OrderItem("p1", 2, 10m)]);
        SetEntityId(order, "order-accept-1");
        var item = order.Items[0];
        SetEntityId(item, "line-accept-1");

        order.Accept();

        order.Status.Should().Be(OrderStatus.Accepted);
        order.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<OrderAcceptedDomainEvent>();
        var ev = (OrderAcceptedDomainEvent)order.DomainEvents.Single();
        ev.OrderId.Should().Be("order-accept-1");
        ev.Lines[0].OrderItemId.Should().Be("line-accept-1");
    }

    [Test]
    public void Accept_when_reopened_sets_accepted()
    {
        var order = new Order("N-A3", "cust-3");
        SetEntityId(order, "order-accept-reopen");
        order.UpdateItems([new OrderItem("p1", 2, 10m)]);
        order.Accept();
        order.Reopen();
        order.ClearDomainEvents();

        order.Accept();

        order.Status.Should().Be(OrderStatus.Accepted);
        order.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<OrderAcceptedDomainEvent>();
    }

    [Test]
    public void Accept_throws_when_not_pending_or_reopened()
    {
        var order = new Order("N-A4", "cust-4");
        order.UpdateItems([new OrderItem("p1", 1, 1m)]);
        order.Accept();

        var act = () => order.Accept();

        act.Should().Throw<InvalidOperationException>();
    }
}
