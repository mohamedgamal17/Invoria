namespace Invoria.Ordering.Contracts.Invoices.Events;

public class CreateOrderInvoiceIntegrationEvent
{
    public string OrderId { get; set; } = null!;
}
