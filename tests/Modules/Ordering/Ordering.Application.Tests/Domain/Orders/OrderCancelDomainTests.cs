using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Contracts.Orders;
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
        order.Accept();

        order.Cancel();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Test]
    public void Cancel_succeeds_when_reopened()
    {
        var order = CreateOrderWithItems("cancel-reopen");
        order.Accept();
        order.Reopen();

        order.Cancel();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Test]
    public void Cancel_throws_when_completed()
    {
        var order = CreateOrderWithItems("cancel-bad-status");
        order.Accept();
        order.MarkInventoryAllocated();
        order.MarkDispatched();
        order.MarkShipped();
        order.Complete();

        var act = () => order.Cancel();

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Cancel_throws_when_shipped()
    {
        var order = CreateOrderWithItems("cancel-shipped");
        order.Accept();
        order.MarkInventoryAllocated();
        order.MarkDispatched();
        order.MarkShipped();

        var act = () => order.Cancel();

        act.Should().Throw<InvalidOperationException>();
    }
}
