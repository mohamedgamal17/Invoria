using Rebus.Sagas;

namespace Invoria.Ordering.Application.Invoices.Sagas;

public sealed class OrderInvoiceSagaState : ISagaData
{
    public Guid Id { get; set; }

    public int Revision { get; set; }

    public string OrderId { get; set; } = default!;

    public string? InvoiceId { get; set; }

    public string State { get; set; } = OrderInvoiceSagaProcessState.Requested;

    public void ApplyRequested(string orderId)
    {
        OrderId = orderId;
        State = OrderInvoiceSagaProcessState.Requested;
    }

    public void ApplyCompleted(string invoiceId)
    {
        InvoiceId = invoiceId;
        State = OrderInvoiceSagaProcessState.Completed;
    }
}
