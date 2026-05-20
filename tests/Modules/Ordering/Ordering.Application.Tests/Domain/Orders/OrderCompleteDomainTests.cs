using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Tests.Domain.Orders;

[TestFixture]
public class OrderCompleteDomainTests
{
    [Test]
    public void Complete_throws_when_accepted_but_not_dispatched()
    {
        var order = new Order("N-C1", "cust");
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(order, "order-complete-alloc");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });
        order.Accept();
        order.MarkInventoryAllocated();

        var act = () => order.Complete();

        act.Should().Throw<InvalidOperationException>();
        order.Status.Should().Be(OrderStatus.Accepted);
    }

    [Test]
    public void Complete_sets_status_to_completed_when_accepted_and_dispatched()
    {
        var order = new Order("N-C2", "cust");
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(order, "order-complete-ok");
        order.UpdateItems(new List<OrderItem> { new("p1", 2, 10m) });
        order.Accept();
        order.MarkInventoryAllocated();
        order.MarkDispatched();
        order.MarkShipped();

        order.Complete();

        order.Status.Should().Be(OrderStatus.Completed);
        order.FullfillmentStatus.Should().Be(FullfillmentStatus.Dispatched);
    }

    [Test]
    public void Complete_cancels_when_all_order_lines_are_fully_returned()
    {
        var order = new Order("N-C3", "cust");
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(order, "order-complete-full-return");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(order.Items[0], "line-1");
        order.Accept();
        order.MarkInventoryAllocated();
        order.MarkDispatched();
        order.MarkShipped();
        order.RecordReturnItems([new OrderReturnItem("line-1", 1)]).IsSuccess.Should().BeTrue();

        order.Complete();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }
}
