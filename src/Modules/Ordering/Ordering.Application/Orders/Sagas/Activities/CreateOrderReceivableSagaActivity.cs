using Invoria.Financial.Contracts.Receivables.Events;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;

namespace Invoria.Ordering.Application.Orders.Sagas.Activities;

public sealed record CreateOrderReceivableSagaActivity(
    string OrderId,
    string CustomerId,
    decimal Amount);

public sealed class CreateOrderReceivableSagaActivityHandler
    : IHandleMessages<CreateOrderReceivableSagaActivity>
{
    private readonly IBus _bus;
    private readonly ILogger<CreateOrderReceivableSagaActivityHandler> _logger;

    public CreateOrderReceivableSagaActivityHandler(
        IBus bus,
        ILogger<CreateOrderReceivableSagaActivityHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public Task Handle(CreateOrderReceivableSagaActivity message)
    {
        _logger.LogDebug(
            "Creating order receivable saga activity for OrderId={OrderId} CustomerId={CustomerId}",
            message.OrderId,
            message.CustomerId);

        var integrationEvent = new CreateOrderReceivableIntegrationEvent
        {
            OrderId = message.OrderId,
            CustomerId = message.CustomerId,
            Amount = message.Amount
        };

        return _bus.Publish(integrationEvent);
    }
}
