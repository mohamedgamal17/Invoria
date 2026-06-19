using Invoria.Ordering.Application.Invoices.Sagas.Activities;
using Invoria.Ordering.Contracts.Invoices.Events;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Invoria.Ordering.Application.Invoices.Sagas;

public sealed class OrderInvoiceSaga : Saga<OrderInvoiceSagaState>,
    IAmInitiatedBy<OrderInvoiceRequestIntegrationEvent>,
    IHandleMessages<OrderInvoiceRequestIntegrationEvent>,
    IHandleMessages<OrderInvoiceCreatedIntegrationEvent>
{
    private readonly IBus _bus;

    public OrderInvoiceSaga(IBus bus)
    {
        _bus = bus;
    }

    protected override void CorrelateMessages(ICorrelationConfig<OrderInvoiceSagaState> config)
    {
        config.Correlate<OrderInvoiceRequestIntegrationEvent>(m => m.OrderId, d => d.OrderId);
        config.Correlate<OrderInvoiceCreatedIntegrationEvent>(m => m.OrderId, d => d.OrderId);
    }

    public async Task Handle(OrderInvoiceRequestIntegrationEvent message)
    {
        Data.ApplyRequested(message.OrderId);

        await _bus.Publish(new CreateOrderInvoiceIntegrationEvent
        {
            OrderId = message.OrderId
        });
    }

    public async Task Handle(OrderInvoiceCreatedIntegrationEvent message)
    {
        Data.ApplyCompleted(message.InvoiceId);

        await _bus.Publish(new RecordOrderInvoiceSagaActivity(message.OrderId, message.InvoiceId));
    }
}
