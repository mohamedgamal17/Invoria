namespace Invoria.Ordering.Contracts.Invoices.Events;

public class OrderInvoiceCreatedIntegrationEvent
{
    public string OrderId { get; set; } = null!;

    public string InvoiceId { get; set; } = null!;
}
