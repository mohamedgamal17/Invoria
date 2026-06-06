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
    public void NetOfTotalOrderAmount_reduces_after_partial_return()
    {
        var order = CreateProcessingOrderWithItems(("line-1", "p1", 3, 10m));

        var result = order.RecordReturnItems([new OrderReturnItem("line-1", 1)]);

        result.IsSuccess.Should().BeTrue();
        order.TotalOrderAmount.Should().Be(30m);
        order.NetOfTotalOrderAmount.Should().Be(20m);
        order.ReturnItems.Should().ContainSingle();
    }

    [Test]
    public void RecordReturnItems_succeeds_in_pending_state()
    {
        var order = new Order("N-R2", "cust");
        SetEntityId(order, "order-return-2");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });
        SetEntityId(order.Items[0], "line-1");
        order.Accept();
        var result = order.RecordReturnItems([new OrderReturnItem("line-1", 1)]);

        result.IsSuccess.Should().BeTrue();
        order.ReturnItems.Should().ContainSingle();
    }

    [Test]
    public void RecordReturnItems_succeeds_when_completed()
    {
        var order = CreateProcessingOrderWithItems(("line-1", "p1", 1, 10m));
        order.Complete();

        var result = order.RecordReturnItems([new OrderReturnItem("line-1", 1)]);

        result.IsSuccess.Should().BeTrue();
        order.ReturnItems.Should().ContainSingle();
    }

    [Test]
    public void RecordReturnItems_replaces_whole_list_on_second_call()
    {
        var order = CreateProcessingOrderWithItems(("line-1", "p1", 3, 10m));

        order.RecordReturnItems([new OrderReturnItem("line-1", 1)]).IsSuccess.Should().BeTrue();
        var result = order.RecordReturnItems([new OrderReturnItem("line-1", 2)]);

        result.IsSuccess.Should().BeTrue();
        order.ReturnItems.Should().ContainSingle();
        order.ReturnItems[0].Quantity.Should().Be(2);
        order.NetOfTotalOrderAmount.Should().Be(10m);
        order.Status.Should().Be(OrderStatus.Processing);
    }

    [Test]
    public void RecordReturnItems_keeps_processing_when_all_lines_fully_returned()
    {
        var order = CreateProcessingOrderWithItems(
            ("line-1", "p1", 2, 10m),
            ("line-2", "p2", 1, 5m));

        var result = order.RecordReturnItems(
        [
            new OrderReturnItem("line-1", 2),
            new OrderReturnItem("line-2", 1)
        ]);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Processing);
    }

    [Test]
    public void RecordReturnItems_applies_multiple_lines_in_one_call()
    {
        var order = CreateProcessingOrderWithItems(
            ("line-1", "p1", 2, 10m),
            ("line-2", "p2", 1, 5m));

        var result = order.RecordReturnItems(
        [
            new OrderReturnItem("line-1", 1),
            new OrderReturnItem("line-2", 1)
        ]);

        result.IsSuccess.Should().BeTrue();
        order.ReturnItems.Should().HaveCount(2);
        order.NetOfTotalOrderAmount.Should().Be(10m);
        order.Status.Should().Be(OrderStatus.Processing);
    }

    [Test]
    public void RecordReturnItems_normalizes_batch_sum_per_line()
    {
        var order = CreateProcessingOrderWithItems(("line-1", "p1", 3, 10m));

        var result = order.RecordReturnItems(
        [
            new OrderReturnItem("line-1", 1),
            new OrderReturnItem("line-1", 2)
        ]);

        result.IsSuccess.Should().BeTrue();
        order.ReturnItems.Should().ContainSingle();
        order.ReturnItems[0].Quantity.Should().Be(3);
        order.NetOfTotalOrderAmount.Should().Be(0m);
        order.Status.Should().Be(OrderStatus.Processing);
    }

    [Test]
    public void RecordReturnItems_clears_returns_when_list_is_empty()
    {
        var order = CreateProcessingOrderWithItems(("line-1", "p1", 2, 10m));
        order.RecordReturnItems([new OrderReturnItem("line-1", 1)]).IsSuccess.Should().BeTrue();

        var result = order.RecordReturnItems([]);

        result.IsSuccess.Should().BeTrue();
        order.ReturnItems.Should().BeEmpty();
        order.NetOfTotalOrderAmount.Should().Be(order.TotalOrderAmount);
        order.Status.Should().Be(OrderStatus.Processing);
    }

    [Test]
    public void RecordReturnItems_replaces_existing_returns_on_second_call()
    {
        var order = CreateProcessingOrderWithItems(("line-1", "p1", 2, 10m));
        order.RecordReturnItems([new OrderReturnItem("line-1", 1)]).IsSuccess.Should().BeTrue();

        var result = order.RecordReturnItems([new OrderReturnItem("line-1", 5)]);

        result.IsSuccess.Should().BeTrue();
        order.ReturnItems.Should().ContainSingle();
        order.ReturnItems[0].Quantity.Should().Be(5);
        order.NetOfTotalOrderAmount.Should().Be(0m);
    }

    [Test]
    public void Complete_cancels_when_all_items_returned()
    {
        var order = CreateProcessingOrderWithItems(("line-1", "p1", 1, 10m));
        order.RecordReturnItems([new OrderReturnItem("line-1", 1)]).IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Processing);

        order.Complete();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Test]
    public void Complete_sets_completed_when_partial_returns()
    {
        var order = CreateProcessingOrderWithItems(("line-1", "p1", 2, 10m));
        order.RecordReturnItems([new OrderReturnItem("line-1", 1)]).IsSuccess.Should().BeTrue();

        order.Complete();

        order.Status.Should().Be(OrderStatus.Completed);
        order.NetOfTotalOrderAmount.Should().Be(10m);
    }

    [Test]
    public void RecordReturnItems_refreshes_AmountOutstanding_to_net_total()
    {
        var order = CreateProcessingOrderWithItems(("line-1", "p1", 3, 10m));
        order.AmountOutstanding.Should().Be(30m);

        order.RecordReturnItems([new OrderReturnItem("line-1", 1)]).IsSuccess.Should().BeTrue();

        order.AmountOutstanding.Should().Be(20m);
        order.AmountOutstanding.Should().Be(order.NetOfTotalOrderAmount);
    }
}
