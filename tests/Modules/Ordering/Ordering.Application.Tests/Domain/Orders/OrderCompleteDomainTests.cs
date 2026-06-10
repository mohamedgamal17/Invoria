using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Domain.Orders.Events;

namespace Invoria.Ordering.Application.Tests.Domain.Orders;

[TestFixture]
public class OrderCompleteDomainTests
{
    private static void SetEntityId(Entity<string> entity, string id)
    {
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(entity, id);
    }

    [Test]
    public void Complete_throws_when_not_processing()
    {
        var order = new Order("N-C1", "cust");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });

        var act = () => order.Complete([]);

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Complete_sets_status_to_completed_when_processing()
    {
        var order = new Order("N-C2", "cust");
        order.UpdateItems(new List<OrderItem> { new("p1", 2, 10m) });
        order.Accept();
        order.Complete([]);

        order.Status.Should().Be(OrderStatus.Completed);
    }

    [Test]
    public void Complete_sets_completed_when_all_order_lines_are_fully_returned()
    {
        var order = new Order("N-C3", "cust");
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(order, "order-complete-full-return");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(order.Items[0], "line-1");
        order.Accept();

        order.Complete([new OrderReturnItem("line-1", 1)]);

        order.Status.Should().Be(OrderStatus.Completed);
        order.ReturnItems.Should().ContainSingle();
        order.NetOfTotalOrderAmount.Should().Be(0m);
    }

    [Test]
    public void Complete_raises_OrderCompletedDomainEvent_with_order_instance()
    {
        var order = new Order("N-C4", "cust");
        order.UpdateItems([new OrderItem("p1", 2, 10m)]);
        SetEntityId(order, "order-complete-1");
        order.Accept();
        order.ClearDomainEvents();

        order.Complete([]);

        order.Status.Should().Be(OrderStatus.Completed);
        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderCompletedDomainEvent>();

        var domainEvent = (OrderCompletedDomainEvent)order.DomainEvents.Single();
        domainEvent.OrderId.Should().Be("order-complete-1");
        domainEvent.Order.Should().BeSameAs(order);
    }

    [Test]
    public void Complete_raises_OrderCompletedDomainEvent_with_post_return_state()
    {
        var order = new Order("N-C5", "cust");
        SetEntityId(order, "order-complete-return");
        order.UpdateItems([new OrderItem("p1", 1, 10m)]);
        SetEntityId(order.Items[0], "line-1");
        order.Accept();
        order.ClearDomainEvents();

        order.Complete([new OrderReturnItem("line-1", 1)]);

        var domainEvent = (OrderCompletedDomainEvent)order.DomainEvents.Single();
        domainEvent.OrderId.Should().Be("order-complete-return");
        domainEvent.Order.Should().BeSameAs(order);
        domainEvent.Order.ReturnItems.Should().ContainSingle();
        domainEvent.Order.NetOfTotalOrderAmount.Should().Be(0m);
    }
}
