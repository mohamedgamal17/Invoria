using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Tests.Domain.Orders;

[TestFixture]
public class OrderReturnItemsDomainTests
{
    private static void SetEntityId(Entity<string> entity, string id)
    {
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(entity, id);
    }

    private static Order CreateProcessingOrderWithItems(params (string lineId, string productId, int qty, decimal price)[] lines)
    {
        var order = new Order("N-R1", "cust");
        SetEntityId(order, "order-return-1");
        var items = lines.Select(l => new OrderItem(l.productId, l.qty, l.price)).ToList();
        order.UpdateItems(items);
        for (var i = 0; i < order.Items.Count; i++)
        {
            SetEntityId(order.Items[i], lines[i].lineId);
        }

        order.Accept();
        return order;
    }

    [Test]
    public void NetOfTotalOrderAmount_equals_gross_when_no_returns()
    {
        var order = CreateProcessingOrderWithItems(("line-1", "p1", 2, 10m));

        order.TotalOrderAmount.Should().Be(20m);
        order.NetOfTotalOrderAmount.Should().Be(20m);
        order.ReturnItems.Should().BeEmpty();
    }

    [Test]
    public void NetOfTotalOrderAmount_reduces_after_partial_return_on_complete()
    {
        var order = CreateProcessingOrderWithItems(("line-1", "p1", 3, 10m));

        order.Complete([new OrderReturnItem("line-1", 1)]);

        order.Status.Should().Be(OrderStatus.Completed);
        order.TotalOrderAmount.Should().Be(30m);
        order.NetOfTotalOrderAmount.Should().Be(20m);
        order.ReturnItems.Should().ContainSingle();
    }

    [Test]
    public void Complete_records_return_items_when_processing()
    {
        var order = CreateProcessingOrderWithItems(("line-1", "p1", 1, 10m));

        order.Complete([new OrderReturnItem("line-1", 1)]);

        order.Status.Should().Be(OrderStatus.Completed);
        order.ReturnItems.Should().ContainSingle();
    }

    [Test]
    public void Complete_applies_multiple_lines_in_one_call()
    {
        var order = CreateProcessingOrderWithItems(
            ("line-1", "p1", 2, 10m),
            ("line-2", "p2", 1, 5m));

        order.Complete(
        [
            new OrderReturnItem("line-1", 1),
            new OrderReturnItem("line-2", 1)
        ]);

        order.Status.Should().Be(OrderStatus.Completed);
        order.ReturnItems.Should().HaveCount(2);
        order.NetOfTotalOrderAmount.Should().Be(10m);
    }

    [Test]
    public void Complete_normalizes_batch_sum_per_line()
    {
        var order = CreateProcessingOrderWithItems(("line-1", "p1", 3, 10m));

        order.Complete(
        [
            new OrderReturnItem("line-1", 1),
            new OrderReturnItem("line-1", 2)
        ]);

        order.Status.Should().Be(OrderStatus.Completed);
        order.ReturnItems.Should().ContainSingle();
        order.ReturnItems[0].Quantity.Should().Be(3);
        order.NetOfTotalOrderAmount.Should().Be(0m);
    }

    [Test]
    public void Complete_with_empty_returns_records_no_return_items()
    {
        var order = CreateProcessingOrderWithItems(("line-1", "p1", 2, 10m));

        order.Complete([]);

        order.Status.Should().Be(OrderStatus.Completed);
        order.ReturnItems.Should().BeEmpty();
        order.NetOfTotalOrderAmount.Should().Be(order.TotalOrderAmount);
    }

    [Test]
    public void Complete_sets_completed_when_all_items_returned()
    {
        var order = CreateProcessingOrderWithItems(("line-1", "p1", 1, 10m));

        order.Complete([new OrderReturnItem("line-1", 1)]);

        order.Status.Should().Be(OrderStatus.Completed);
        order.NetOfTotalOrderAmount.Should().Be(0m);
    }

    [Test]
    public void Complete_sets_completed_when_partial_returns()
    {
        var order = CreateProcessingOrderWithItems(("line-1", "p1", 2, 10m));

        order.Complete([new OrderReturnItem("line-1", 1)]);

        order.Status.Should().Be(OrderStatus.Completed);
        order.NetOfTotalOrderAmount.Should().Be(10m);
    }

    [Test]
    public void Complete_refreshes_AmountOutstanding_to_net_total()
    {
        var order = CreateProcessingOrderWithItems(("line-1", "p1", 3, 10m));
        order.AmountOutstanding.Should().Be(30m);

        order.Complete([new OrderReturnItem("line-1", 1)]);

        order.AmountOutstanding.Should().Be(20m);
        order.AmountOutstanding.Should().Be(order.NetOfTotalOrderAmount);
    }
}
