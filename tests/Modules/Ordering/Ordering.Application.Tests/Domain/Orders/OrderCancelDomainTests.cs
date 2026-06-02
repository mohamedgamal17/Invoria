using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Tests.Domain.Orders;

[TestFixture]
public class OrderCancelDomainTests
{
    private static Order CreateOrderWithItems(string id)
    {
        var order = new Order("CN-1", "cust");
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(order, id);
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });
        return order;
    }

    [Test]
    public void Cancel_succeeds_when_pending()
    {
        var order = CreateOrderWithItems("cancel-pending");

        order.Cancel();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Test]
    public void Cancel_succeeds_when_accepted()
    {
        var order = CreateOrderWithItems("cancel-acc");
        order.Revise();

        order.Cancel();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Test]
    public void Cancel_succeeds_when_revised()
    {
        var order = CreateOrderWithItems("cancel-revise");
        typeof(Order).GetProperty(nameof(Order.Status))!.SetValue(order, OrderStatus.Revision);

        order.Cancel();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Test]
    public void Cancel_throws_when_completed()
    {
        var order = CreateOrderWithItems("cancel-bad-status");
        order.Revise();
        order.Complete();

        var act = () => order.Cancel();

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Cancel_succeeds_when_processing()
    {
        var order = CreateOrderWithItems("cancel-shipped");
        order.Revise();

        order.Cancel();
        order.Status.Should().Be(OrderStatus.Cancelled);
    }
}
