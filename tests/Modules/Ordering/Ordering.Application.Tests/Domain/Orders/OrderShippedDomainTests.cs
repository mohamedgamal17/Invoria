using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Contracts.Orders;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Tests.Domain.Orders;

[TestFixture]
public class OrderShippedDomainTests
{
    private static Order CreateAcceptedDispatchedOrder()
    {
        var order = new Order("N-S1", "cust");
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(order, "order-shipped-1");
        order.UpdateItems(new List<OrderItem> { new("p1", 2, 10m) });
        order.Accept();
        order.MarkInventoryAllocated();
        order.MarkDispatched();
        return order;
    }

    [Test]
    public void MarkShipped_sets_status_to_shipped_when_accepted()
    {
        var order = CreateAcceptedDispatchedOrder();

        order.MarkShipped();

        order.Status.Should().Be(OrderStatus.Shipped);
    }

    [Test]
    public void MarkShipped_is_idempotent_when_already_shipped()
    {
        var order = CreateAcceptedDispatchedOrder();
        order.MarkShipped();

        var act = () => order.MarkShipped();

        act.Should().NotThrow();
        order.Status.Should().Be(OrderStatus.Shipped);
    }

    [Test]
    public void Complete_throws_when_not_shipped()
    {
        var order = CreateAcceptedDispatchedOrder();

        var act = () => order.Complete();

        act.Should().Throw<InvalidOperationException>();
        order.Status.Should().Be(OrderStatus.Accepted);
    }

    [Test]
    public void Complete_sets_status_to_completed_when_shipped()
    {
        var order = CreateAcceptedDispatchedOrder();
        order.MarkShipped();

        order.Complete();

        order.Status.Should().Be(OrderStatus.Completed);
    }
}
