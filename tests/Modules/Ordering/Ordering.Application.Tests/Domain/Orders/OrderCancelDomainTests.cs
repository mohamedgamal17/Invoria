using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
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
    public void Cancel_succeeds_when_pending_and_fulfillment_pending()
    {
        var order = CreateOrderWithItems("cancel-pending");

        order.Cancel();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Test]
    public void Cancel_succeeds_when_accepted_and_fulfillment_on_hold()
    {
        var order = CreateOrderWithItems("cancel-acc-hold");
        order.Accept();
        order.FullfillmentStatus = FullfillmentStatus.OnHold;

        order.Cancel();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Test]
    public void Cancel_succeeds_when_accepted_and_fulfillment_allocated()
    {
        var order = CreateOrderWithItems("cancel-acc-alloc");
        order.Accept();
        order.MarkInventoryAllocated();

        order.Cancel();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Test]
    public void Cancel_succeeds_when_reopened_and_fulfillment_on_hold()
    {
        var order = CreateOrderWithItems("cancel-reopen");
        order.Accept();
        order.Reopen();
        order.CompleteReopenAfterInventoryReleased();

        order.Cancel();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Test]
    public void Cancel_throws_when_completed_even_if_fulfillment_pending()
    {
        var order = CreateOrderWithItems("cancel-bad-status");
        order.Accept();
        order.MarkInventoryAllocated();
        order.MarkDispatched();
        order.Complete();

        var act = () => order.Cancel();

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Cancel_throws_when_accepted_and_fulfillment_allocating()
    {
        var order = CreateOrderWithItems("cancel-allocating");
        order.Accept();

        var act = () => order.Cancel();

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Cancel_throws_when_accepted_and_fulfillment_dispatched()
    {
        var order = CreateOrderWithItems("cancel-dispatched");
        order.Accept();
        order.MarkInventoryAllocated();
        order.MarkDispatched();

        var act = () => order.Cancel();

        act.Should().Throw<InvalidOperationException>();
    }
}
