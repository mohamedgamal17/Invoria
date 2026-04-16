using Invoria.Inventory.Application.Batches.Commands.CreateBatchesFromPurchaseOrderCompleted;
using MediatR;
using Invoria.Procurement.Contracts.Events;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Invoria.Inventory.Application.Batches.Consumers;

public sealed class PurchaseOrderCompletedIntegrationEventConsumer
    : IHandleMessages<PurchaseOrderCompletedIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<PurchaseOrderCompletedIntegrationEventConsumer> _logger;

    public PurchaseOrderCompletedIntegrationEventConsumer(
        IMediator mediator,
        ILogger<PurchaseOrderCompletedIntegrationEventConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(PurchaseOrderCompletedIntegrationEvent message)
    {
        _logger.LogDebug(
            "Consuming integration event {EventName} for PurchaseOrderId={PurchaseOrderId} PurchaseNumber={PurchaseNumber}",
            nameof(PurchaseOrderCompletedIntegrationEvent),
            message.PurchaseOrderId,
            message.PurchaseNumber);

        var result = await _mediator.Send(
            CreateBatchesFromPurchaseOrderCompletedCommand.FromEvent(message),
            CancellationToken.None);

        if (result.IsFailure)
        {
            throw result.Exception!;
        }
    }
}
