using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Tests.Domain.Orders;

[TestFixture]
public class OrderBillableItemsDomainTests
{
    private static void SetEntityId(Entity<string> entity, string id)
    {
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(entity, id);
    }

    private static Order CreateProcessingOrderWithItems(params (string lineId, string productId, int qty, decimal price)[] lines)
    {
        var order = new Order("N-BILL", "cust");
        SetEntityId(order, "order-bill-1");
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
    public void GetBillableItems_returns_all_lines_when_no_returns()
    {
        var order = CreateProcessingOrderWithItems(("line-1", "p1", 2, 10m));

        order.Complete([]);

        var billable = order.GetBillableItems().ToList();

        billable.Should().ContainSingle();
        billable[0].Line.Id.Should().Be("line-1");
        billable[0].BillableQuantity.Should().Be(2);
    }

    [Test]
    public void GetBillableItems_reduces_quantity_after_partial_return()
    {
        var order = CreateProcessingOrderWithItems(("line-1", "p1", 3, 10m));

        order.Complete([new OrderReturnItem("line-1", 1)]);

        var billable = order.GetBillableItems().ToList();

        billable.Should().ContainSingle();
        billable[0].BillableQuantity.Should().Be(2);
    }

    [Test]
    public void GetBillableItems_omits_fully_returned_line()
    {
        var order = CreateProcessingOrderWithItems(("line-1", "p1", 2, 10m));

        order.Complete([new OrderReturnItem("line-1", 2)]);

        order.GetBillableItems().Should().BeEmpty();
    }

    [Test]
    public void GetBillableItems_handles_mixed_lines()
    {
        var order = CreateProcessingOrderWithItems(
            ("line-1", "p1", 2, 10m),
            ("line-2", "p2", 1, 5m));

        order.Complete(
        [
            new OrderReturnItem("line-1", 1),
            new OrderReturnItem("line-2", 1)
        ]);

        var billable = order.GetBillableItems().ToList();

        billable.Should().ContainSingle();
        billable[0].Line.Id.Should().Be("line-1");
        billable[0].BillableQuantity.Should().Be(1);
    }

    [Test]
    public void GetBillableItems_line_totals_match_NetOfTotalOrderAmount()
    {
        var order = CreateProcessingOrderWithItems(
            ("line-1", "p1", 3, 10m),
            ("line-2", "p2", 2, 5m));

        order.Complete([new OrderReturnItem("line-1", 1)]);

        var billableTotal = order.GetBillableItems()
            .Sum(b => b.Line.Price * b.BillableQuantity);

        billableTotal.Should().Be(order.NetOfTotalOrderAmount);
    }
}
