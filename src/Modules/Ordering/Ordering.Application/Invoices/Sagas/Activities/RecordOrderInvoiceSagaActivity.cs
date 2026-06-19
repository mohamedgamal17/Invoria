using Invoria.Ordering.Application.Orders.Commands.RecordOrderInvoice;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Invoria.Ordering.Application.Invoices.Sagas.Activities;

public sealed record RecordOrderInvoiceSagaActivity(string OrderId, string InvoiceId);

public sealed class RecordOrderInvoiceSagaActivityHandler
    : IHandleMessages<RecordOrderInvoiceSagaActivity>
{
    private readonly IMediator _mediator;
    private readonly ILogger<RecordOrderInvoiceSagaActivityHandler> _logger;

    public RecordOrderInvoiceSagaActivityHandler(
        IMediator mediator,
        ILogger<RecordOrderInvoiceSagaActivityHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public Task Handle(RecordOrderInvoiceSagaActivity message)
    {
        _logger.LogDebug(
            "Recording order invoice saga activity for OrderId={OrderId} InvoiceId={InvoiceId}",
            message.OrderId,
            message.InvoiceId);

        return _mediator.Send(
            new RecordOrderInvoiceCommand(message.OrderId, message.InvoiceId),
            CancellationToken.None);
    }
}
