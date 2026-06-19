using Invoria.Ordering.Contracts.Invoices.Events;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;

namespace Invoria.Ordering.Application.Orders.Sagas.Activities;

public sealed record CreateOrderInvoiceSagaActivity(string OrderId);

public sealed class CreateOrderInvoiceSagaActivityHandler
    : IHandleMessages<CreateOrderInvoiceSagaActivity>
{
    private readonly IBus _bus;
    private readonly ILogger<CreateOrderInvoiceSagaActivityHandler> _logger;

    public CreateOrderInvoiceSagaActivityHandler(
        IBus bus,
        ILogger<CreateOrderInvoiceSagaActivityHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public Task Handle(CreateOrderInvoiceSagaActivity message)
    {
        _logger.LogDebug(
            "Creating order invoice saga activity for OrderId={OrderId}",
            message.OrderId);

        return _bus.Publish(new OrderInvoiceRequestIntegrationEvent
        {
            OrderId = message.OrderId
        });
    }
}
