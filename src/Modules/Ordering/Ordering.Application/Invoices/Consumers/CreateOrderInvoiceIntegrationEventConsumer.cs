using Invoria.Ordering.Application.Invoices.Commands.CreateInvoice;
using Invoria.Ordering.Contracts.Invoices.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;

namespace Invoria.Ordering.Application.Invoices.Consumers;

public class CreateOrderInvoiceIntegrationEventConsumer
    : IHandleMessages<CreateOrderInvoiceIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly IBus _bus;
    private readonly ILogger<CreateOrderInvoiceIntegrationEventConsumer> _logger;

    public CreateOrderInvoiceIntegrationEventConsumer(
        IMediator mediator,
        IBus bus,
        ILogger<CreateOrderInvoiceIntegrationEventConsumer> logger)
    {
        _mediator = mediator;
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(CreateOrderInvoiceIntegrationEvent message)
    {
        _logger.LogDebug(
            "Consuming integration event {EventName} for OrderId={OrderId}",
            nameof(CreateOrderInvoiceIntegrationEvent),
            message.OrderId);

        var result = await _mediator.Send(new CreateInvoiceCommand(message.OrderId), CancellationToken.None);

        if (!result.IsSuccess || result.Value is null)
        {
            throw new InvalidOperationException(
                $"Failed to create invoice for order {message.OrderId}.");
        }

        await _bus.Publish(new OrderInvoiceCreatedIntegrationEvent
        {
            OrderId = message.OrderId,
            InvoiceId = result.Value.Id
        });
    }
}
