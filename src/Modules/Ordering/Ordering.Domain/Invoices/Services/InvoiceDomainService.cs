using Ardalis.GuardClauses;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Domain.Invoices.Services;

public class InvoiceDomainService : IInvoiceDomainService
{
    public Invoice CreateFromOrder(Order order)
    {
        Guard.Against.Null(order);

        var billableItems = order.GetBillableItems().ToList();
        if (billableItems.Count == 0)
        {
            throw new InvalidOperationException("Order has no billable items to invoice.");
        }

        var items = billableItems
            .Select(b => new InvoiceItem(b.Line.Id, b.Line.ProductId, b.BillableQuantity, b.Line.Price))
            .ToList();

        var total = items.Sum(i => i.Price * i.Quantity);

        return Invoice.Create(order.CustomerId, order.Id, total, total, items);
    }
}
