using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Contracts.Orders;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Tests.Domain.Orders;

[TestFixture]
public class OrderAcceptDomainTests
{
    private static void SetEntityId(Entity<string> entity, string id)
    {
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(entity, id);
    }

    [Test]
    public void Revise_sets_processing()
    {
        var order = new Order("TEST-1", Guid.NewGuid().ToString());
        order.UpdateItems([new OrderItem("p1", 2, 10m)]);
        SetEntityId(order, "order-accept-1");
        var item = order.Items[0];
        SetEntityId(item, "line-accept-1");

        order.Revise();

        order.Status.Should().Be(OrderStatus.Processing);
        order.DomainEvents.Should().BeEmpty();
    }

    [Test]
    public void Revise_when_revision_sets_processing()
    {
        var order = new Order("N-A3", "cust-3");
        SetEntityId(order, "order-accept-revise");
        order.UpdateItems([new OrderItem("p1", 2, 10m)]);
        typeof(Order).GetProperty(nameof(Order.Status))!.SetValue(order, OrderStatus.Revision);
        order.ClearDomainEvents();

        order.Revise();

        order.Status.Should().Be(OrderStatus.Processing);
        order.DomainEvents.Should().BeEmpty();
    }

    [Test]
    public void Revise_throws_when_not_pending_or_revision()
    {
        var order = new Order("N-A4", "cust-4");
        order.UpdateItems([new OrderItem("p1", 1, 1m)]);
        order.Revise();

        var act = () => order.Revise();

        act.Should().Throw<InvalidOperationException>();
    }
}
