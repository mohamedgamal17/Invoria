using FluentAssertions;
using Invoria.Ordering.Domain.Invoices;

namespace Invoria.Ordering.Application.Tests.Domain.Invoices;

[TestFixture]
public class InvoiceTests
{
    [Test]
    public void Create_sets_header_and_items()
    {
        const string customerId = "cust-invoice-1";
        const string orderId = "order-invoice-1";
        const decimal subtotal = 20m;
        const decimal totalPrice = 22m;

        var invoice = Invoice.Create(
            customerId,
            orderId,
            subtotal,
            totalPrice,
            [new InvoiceItem("p1", 2, 10m)]);

        invoice.Id.Should().NotBeNullOrWhiteSpace();
        invoice.CustomerId.Should().Be(customerId);
        invoice.OrderId.Should().Be(orderId);
        invoice.Subtotal.Should().Be(subtotal);
        invoice.TotalPrice.Should().Be(totalPrice);
        invoice.Items.Should().ContainSingle()
            .Which.ProductId.Should().Be("p1");
    }

    [Test]
    public void Create_requires_at_least_one_item()
    {
        var act = () => Invoice.Create(
            "cust-1",
            "order-1",
            10m,
            10m,
            []);

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Create_rejects_negative_subtotal_or_total_price()
    {
        var actSubtotal = () => Invoice.Create(
            "cust-1",
            "order-1",
            -1m,
            10m,
            [new InvoiceItem("p1", 1, 10m)]);

        actSubtotal.Should().Throw<ArgumentException>();

        var actTotalPrice = () => Invoice.Create(
            "cust-1",
            "order-1",
            10m,
            -1m,
            [new InvoiceItem("p1", 1, 10m)]);

        actTotalPrice.Should().Throw<ArgumentException>();
    }
}
