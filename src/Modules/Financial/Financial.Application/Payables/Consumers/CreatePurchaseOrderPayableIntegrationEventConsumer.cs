using Invoria.Financial.Application.Payables.Commands.CreatePayable;
using Invoria.Financial.Contracts.Payables.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Invoria.Financial.Application.Payables.Consumers;

public class CreatePurchaseOrderPayableIntegrationEventConsumer
    : IHandleMessages<CreatePurchaseOrderPayableIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<CreatePurchaseOrderPayableIntegrationEventConsumer> _logger;

    public CreatePurchaseOrderPayableIntegrationEventConsumer(
        IMediator mediator,
        ILogger<CreatePurchaseOrderPayableIntegrationEventConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public Task Handle(CreatePurchaseOrderPayableIntegrationEvent message)
    {
        _logger.LogDebug(
            "Consuming integration event {EventName} for PurchaseOrderId={PurchaseOrderId} SupplierId={SupplierId}",
            nameof(CreatePurchaseOrderPayableIntegrationEvent),
            message.PurchaseOrderId,
            message.SupplierId);

        var command = CreatePayableCommand.FromEvent(message);

        return _mediator.Send(command, CancellationToken.None);
    }
}
