using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Ordering.Contracts.Orders;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Tests.Domain.Orders;

[TestFixture]
public class OrderReturnItemsDomainTests
{
    private static void SetEntityId(Entity<string> entity, string id)
    {
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(entity, id);
    }

    private static Order CreateShippedOrderWithItems(params (string lineId, string productId, int qty, decimal price)[] lines)
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
        order.MarkInventoryAllocated();
        order.MarkDispatched();
        order.MarkShipped();
        return order;
    }

    [Test]
    public void NetOfTotalOrderAmount_equals_gross_when_no_returns()
    {
        var order = CreateShippedOrderWithItems(("line-1", "p1", 2, 10m));

        order.TotalOrderAmount.Should().Be(20m);
        order.NetOfTotalOrderAmount.Should().Be(20m);
        order.ReturnItems.Should().BeEmpty();
    }

    [Test]
    public void NetOfTotalOrderAmount_reduces_after_partial_return()
    {
        var order = CreateShippedOrderWithItems(("line-1", "p1", 3, 10m));

        var result = order.RecordReturnItems([new OrderReturnItem("line-1", 1)]);

        result.IsSuccess.Should().BeTrue();
        order.TotalOrderAmount.Should().Be(30m);
        order.NetOfTotalOrderAmount.Should().Be(20m);
        order.ReturnItems.Should().ContainSingle();
    }

    [Test]
    public void RecordReturnItems_fails_when_not_shipped()
    {
        var order = new Order("N-R2", "cust");
        SetEntityId(order, "order-return-2");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });
        SetEntityId(order.Items[0], "line-1");
        order.Accept();
        order.MarkInventoryAllocated();
        order.MarkDispatched();

        var result = order.RecordReturnItems([new OrderReturnItem("line-1", 1)]);

        result.IsFailure.Should().BeTrue();
        result.Exception.Should().BeOfType<BusinessValidationException>();
        ((BusinessValidationException)result.Exception!).Messages.Should()
            .Contain("Return items can only be recorded when the order is Shipped.");
        order.ReturnItems.Should().BeEmpty();
    }

    [Test]
    public void RecordReturnItems_fails_when_completed()
    {
        var order = CreateShippedOrderWithItems(("line-1", "p1", 1, 10m));
        order.Complete();

        var result = order.RecordReturnItems([new OrderReturnItem("line-1", 1)]);

        result.IsFailure.Should().BeTrue();
        result.Exception.Should().BeOfType<BusinessValidationException>();
        order.ReturnItems.Should().BeEmpty();
    }

    [Test]
    public void RecordReturnItems_fails_for_unknown_order_item_id()
    {
        var order = CreateShippedOrderWithItems(("line-1", "p1", 1, 10m));

        var result = order.RecordReturnItems([new OrderReturnItem("unknown-line", 1)]);

        result.IsFailure.Should().BeTrue();
        var validation = (BusinessValidationException)result.Exception!;
        validation.Messages.Should().Contain(m => m.Contains("unknown-line"));
        order.ReturnItems.Should().BeEmpty();
    }

    [Test]
    public void RecordReturnItems_fails_when_return_quantity_exceeds_line_quantity()
    {
        var order = CreateShippedOrderWithItems(("line-1", "p1", 2, 10m));

        var result = order.RecordReturnItems([new OrderReturnItem("line-1", 3)]);

        result.IsFailure.Should().BeTrue();
        var validation = (BusinessValidationException)result.Exception!;
        validation.Messages.Should().Contain(m => m.Contains("line-1") && m.Contains("ordered quantity"));
        order.ReturnItems.Should().BeEmpty();
    }

    [Test]
    public void RecordReturnItems_replaces_whole_list_on_second_call()
    {
        var order = CreateShippedOrderWithItems(("line-1", "p1", 3, 10m));

        order.RecordReturnItems([new OrderReturnItem("line-1", 1)]).IsSuccess.Should().BeTrue();
        var result = order.RecordReturnItems([new OrderReturnItem("line-1", 2)]);

        result.IsSuccess.Should().BeTrue();
        order.ReturnItems.Should().ContainSingle();
        order.ReturnItems[0].Quantity.Should().Be(2);
        order.NetOfTotalOrderAmount.Should().Be(10m);
        order.Status.Should().Be(OrderStatus.Shipped);
    }

    [Test]
    public void RecordReturnItems_keeps_shipped_when_all_lines_fully_returned()
    {
        var order = CreateShippedOrderWithItems(
            ("line-1", "p1", 2, 10m),
            ("line-2", "p2", 1, 5m));

        var result = order.RecordReturnItems(
        [
            new OrderReturnItem("line-1", 2),
            new OrderReturnItem("line-2", 1)
        ]);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Shipped);
    }

    [Test]
    public void RecordReturnItems_applies_multiple_lines_in_one_call()
    {
        var order = CreateShippedOrderWithItems(
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
        order.Status.Should().Be(OrderStatus.Shipped);
    }

    [Test]
    public void RecordReturnItems_normalizes_batch_sum_per_line()
    {
        var order = CreateShippedOrderWithItems(("line-1", "p1", 3, 10m));

        var result = order.RecordReturnItems(
        [
            new OrderReturnItem("line-1", 1),
            new OrderReturnItem("line-1", 2)
        ]);

        result.IsSuccess.Should().BeTrue();
        order.ReturnItems.Should().ContainSingle();
        order.ReturnItems[0].Quantity.Should().Be(3);
        order.NetOfTotalOrderAmount.Should().Be(0m);
        order.Status.Should().Be(OrderStatus.Shipped);
    }

    [Test]
    public void RecordReturnItems_validates_batch_sum_per_line_when_over_ordered_quantity()
    {
        var order = CreateShippedOrderWithItems(("line-1", "p1", 2, 10m));

        var result = order.RecordReturnItems(
        [
            new OrderReturnItem("line-1", 1),
            new OrderReturnItem("line-1", 2)
        ]);

        result.IsFailure.Should().BeTrue();
        order.ReturnItems.Should().BeEmpty();
    }

    [Test]
    public void RecordReturnItems_clears_returns_when_list_is_empty()
    {
        var order = CreateShippedOrderWithItems(("line-1", "p1", 2, 10m));
        order.RecordReturnItems([new OrderReturnItem("line-1", 1)]).IsSuccess.Should().BeTrue();

        var result = order.RecordReturnItems([]);

        result.IsSuccess.Should().BeTrue();
        order.ReturnItems.Should().BeEmpty();
        order.NetOfTotalOrderAmount.Should().Be(order.TotalOrderAmount);
        order.Status.Should().Be(OrderStatus.Shipped);
    }

    [Test]
    public void RecordReturnItems_failed_replace_does_not_mutate_existing_returns()
    {
        var order = CreateShippedOrderWithItems(("line-1", "p1", 2, 10m));
        order.RecordReturnItems([new OrderReturnItem("line-1", 1)]).IsSuccess.Should().BeTrue();

        var result = order.RecordReturnItems([new OrderReturnItem("line-1", 5)]);

        result.IsFailure.Should().BeTrue();
        order.ReturnItems.Should().ContainSingle();
        order.ReturnItems[0].Quantity.Should().Be(1);
        order.NetOfTotalOrderAmount.Should().Be(10m);
    }

    [Test]
    public void RecordReturnItems_collects_multiple_validation_messages()
    {
        var order = CreateShippedOrderWithItems(("line-1", "p1", 1, 10m));

        var result = order.RecordReturnItems(
        [
            new OrderReturnItem("unknown-line", 1),
            new OrderReturnItem("line-1", 5)
        ]);

        result.IsFailure.Should().BeTrue();
        var validation = (BusinessValidationException)result.Exception!;
        validation.Messages.Should().HaveCountGreaterThanOrEqualTo(2);
        validation.Messages.Should().Contain(m => m.Contains("unknown-line"));
        validation.Messages.Should().Contain(m => m.Contains("line-1") && m.Contains("ordered quantity"));
        order.ReturnItems.Should().BeEmpty();
    }

    [Test]
    public void Complete_cancels_when_all_items_returned()
    {
        var order = CreateShippedOrderWithItems(("line-1", "p1", 1, 10m));
        order.RecordReturnItems([new OrderReturnItem("line-1", 1)]).IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Shipped);

        order.Complete();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Test]
    public void Complete_sets_completed_when_partial_returns()
    {
        var order = CreateShippedOrderWithItems(("line-1", "p1", 2, 10m));
        order.RecordReturnItems([new OrderReturnItem("line-1", 1)]).IsSuccess.Should().BeTrue();

        order.Complete();

        order.Status.Should().Be(OrderStatus.Completed);
        order.NetOfTotalOrderAmount.Should().Be(10m);
    }

    [Test]
    public void RecordReturnItems_refreshes_AmountOutstanding_to_net_total()
    {
        var order = CreateShippedOrderWithItems(("line-1", "p1", 3, 10m));
        order.AmountOutstanding.Should().Be(30m);

        order.RecordReturnItems([new OrderReturnItem("line-1", 1)]).IsSuccess.Should().BeTrue();

        order.AmountOutstanding.Should().Be(20m);
        order.AmountOutstanding.Should().Be(order.NetOfTotalOrderAmount);
    }
}
