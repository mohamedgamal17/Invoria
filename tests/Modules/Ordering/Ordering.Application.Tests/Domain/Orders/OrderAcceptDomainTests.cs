using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Contracts.Orders.Enums;
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
    public void Accept_sets_processing_and_raises_domain_event()
    {
        var order = new Order("TEST-1", Guid.NewGuid().ToString());
        order.UpdateItems([new OrderItem("p1", 2, 10m)]);
        SetEntityId(order, "order-accept-1");
        var item = order.Items[0];
        SetEntityId(item, "line-accept-1");

        order.Accept();

        order.Status.Should().Be(OrderStatus.Processing);
        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderAcceptedDomainEvent>()
            .Which.Order.Should().BeSameAs(order);
    }

    [Test]
    public void Accept_when_revision_sets_processing_and_raises_domain_event()
    {
        var order = new Order("N-A3", "cust-3");
        SetEntityId(order, "order-accept-revise");
        order.UpdateItems([new OrderItem("p1", 2, 10m)]);
        typeof(Order).GetProperty(nameof(Order.Status))!.SetValue(order, OrderStatus.Revision);
        order.ClearDomainEvents();

        order.Accept();

        order.Status.Should().Be(OrderStatus.Processing);
        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderAcceptedDomainEvent>()
            .Which.Order.Should().BeSameAs(order);
    }

    [Test]
    public void Accept_throws_when_not_pending_or_revision()
    {
        var order = new Order("N-A4", "cust-4");
        order.UpdateItems([new OrderItem("p1", 1, 1m)]);
        order.Accept();

        var act = () => order.Accept();

        act.Should().Throw<InvalidOperationException>();
        order.DomainEvents.Should().HaveCount(1);
    }
}
