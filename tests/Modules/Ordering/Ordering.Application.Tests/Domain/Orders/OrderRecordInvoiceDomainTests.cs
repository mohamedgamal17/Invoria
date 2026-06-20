using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Tests.Domain.Orders;

[TestFixture]
public class OrderRecordInvoiceDomainTests
{
    private static void SetEntityId(Entity<string> entity, string id)
    {
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(entity, id);
    }

    [Test]
    public void RecordInvoice_sets_invoice_id()
    {
        var order = new Order("TEST-INV-1", Guid.NewGuid().ToString());
        order.UpdateItems([new OrderItem("p1", 2, 10m)]);
        SetEntityId(order, "order-1");

        order.RecordInvoice("invoice-1");

        order.InvoiceId.Should().Be("invoice-1");
    }

    [Test]
    public void RecordInvoice_throws_when_invoice_already_recorded()
    {
        var order = new Order("TEST-INV-2", Guid.NewGuid().ToString());
        order.UpdateItems([new OrderItem("p1", 1, 10m)]);
        order.RecordInvoice("invoice-1");

        var act = () => order.RecordInvoice("invoice-2");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already has an invoice*");
        order.InvoiceId.Should().Be("invoice-1");
    }
}
