using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Contracts.Orders;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Tests.Domain.Orders;

[TestFixture]
public class OrderShippedDomainTests
{
    private static void SetEntityId(Entity<string> entity, string id)
    {
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(entity, id);
    }

    private static Order CreateAcceptedDispatchedOrder()
    {
        var order = new Order("N-S1", "cust");
        SetEntityId(order, "order-shipped-1");
        order.UpdateItems(new List<OrderItem> { new("p1", 2, 10m) });
        order.Accept();
        order.MarkInventoryAllocated();
        order.MarkDispatched();
        return order;
    }

    [Test]
    public void MarkShipped_throws_when_not_dispatched()
    {
        var order = new Order("N-S2", "cust");
        SetEntityId(order, "order-shipped-2");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });
        order.Accept();
        order.MarkInventoryAllocated();

        var act = () => order.MarkShipped();

        act.Should().Throw<InvalidOperationException>();
        order.Status.Should().Be(OrderStatus.Accepted);
    }

    [Test]
    public void MarkShipped_sets_status_to_shipped_when_accepted_and_dispatched()
    {
        var order = CreateAcceptedDispatchedOrder();

        order.MarkShipped();

        order.Status.Should().Be(OrderStatus.Shipped);
        order.FullfillmentStatus.Should().Be(FullfillmentStatus.Dispatched);
    }

    [Test]
    public void MarkShipped_is_idempotent_when_already_shipped()
    {
        var order = CreateAcceptedDispatchedOrder();
        order.MarkShipped();
        order.ClearDomainEvents();

        var act = () => order.MarkShipped();

        act.Should().NotThrow();
        order.Status.Should().Be(OrderStatus.Shipped);
        order.FullfillmentStatus.Should().Be(FullfillmentStatus.Dispatched);
    }

    [Test]
    public void Complete_throws_when_dispatched_but_not_shipped()
    {
        var order = CreateAcceptedDispatchedOrder();

        var act = () => order.Complete();

        act.Should().Throw<InvalidOperationException>();
        order.Status.Should().Be(OrderStatus.Accepted);
    }

    [Test]
    public void Complete_sets_status_to_completed_when_shipped_and_dispatched()
    {
        var order = CreateAcceptedDispatchedOrder();
        order.MarkShipped();

        order.Complete();

        order.Status.Should().Be(OrderStatus.Completed);
        order.FullfillmentStatus.Should().Be(FullfillmentStatus.Dispatched);
    }
}
