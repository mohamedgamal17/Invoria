using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Domain.Invoices.Services;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Tests.Domain.Invoices;

[TestFixture]
public class InvoiceDomainServiceTests
{
    private readonly InvoiceDomainService _sut = new();

    private static void SetEntityId(Entity<string> entity, string id)
    {
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(entity, id);
    }

    private static Order CreateCompletedOrderWithItems(params (string lineId, string productId, int qty, decimal price)[] lines)
    {
        var order = new Order("N-INV", "cust-invoice");
        SetEntityId(order, "order-inv-1");
        var items = lines.Select(l => new OrderItem(l.productId, l.qty, l.price)).ToList();
        order.UpdateItems(items);
        for (var i = 0; i < order.Items.Count; i++)
        {
            SetEntityId(order.Items[i], lines[i].lineId);
        }

        order.Accept();
        order.Complete([]);
        return order;
    }

    [Test]
    public void CreateFromOrder_maps_billable_lines_with_order_item_id()
    {
        var order = CreateCompletedOrderWithItems(("line-1", "p1", 2, 10m));

        var invoice = _sut.CreateFromOrder(order);

        invoice.CustomerId.Should().Be("cust-invoice");
        invoice.OrderId.Should().Be("order-inv-1");
        invoice.Items.Should().ContainSingle();
        invoice.Items[0].OrderItemId.Should().Be("line-1");
        invoice.Items[0].ProductId.Should().Be("p1");
        invoice.Items[0].Quantity.Should().Be(2);
        invoice.Items[0].Price.Should().Be(10m);
        invoice.Subtotal.Should().Be(20m);
        invoice.TotalPrice.Should().Be(20m);
    }

    [Test]
    public void CreateFromOrder_deducts_returned_quantities()
    {
        var order = new Order("N-INV2", "cust-invoice");
        SetEntityId(order, "order-inv-2");
        order.UpdateItems([new OrderItem("p1", 3, 10m)]);
        SetEntityId(order.Items[0], "line-1");
        order.Accept();
        order.Complete([new OrderReturnItem("line-1", 1)]);

        var invoice = _sut.CreateFromOrder(order);

        invoice.Items.Should().ContainSingle();
        invoice.Items[0].Quantity.Should().Be(2);
        invoice.TotalPrice.Should().Be(20m);
        invoice.TotalPrice.Should().Be(order.NetOfTotalOrderAmount);
    }

    [Test]
    public void CreateFromOrder_throws_when_all_items_returned()
    {
        var order = new Order("N-INV3", "cust-invoice");
        SetEntityId(order, "order-inv-3");
        order.UpdateItems([new OrderItem("p1", 1, 10m)]);
        SetEntityId(order.Items[0], "line-1");
        order.Accept();
        order.Complete([new OrderReturnItem("line-1", 1)]);

        var act = () => _sut.CreateFromOrder(order);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*no billable items*");
    }
}
