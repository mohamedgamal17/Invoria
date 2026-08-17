using Invoria.Procurement.Application.ReportPurchaseOrdersCompletedMetrics.Commands.RecordPurchaseOrdersCompletedMetrics;
using Invoria.Procurement.Contracts.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Invoria.Procurement.Application.ReportPurchaseOrdersCompletedMetrics.Consumers;

public sealed class RecordPurchaseOrdersCompletedMetricsIntegrationEventConsumer
    : IHandleMessages<PurchaseOrderCompletedIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<RecordPurchaseOrdersCompletedMetricsIntegrationEventConsumer> _logger;

    public RecordPurchaseOrdersCompletedMetricsIntegrationEventConsumer(
        IMediator mediator,
        ILogger<RecordPurchaseOrdersCompletedMetricsIntegrationEventConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(PurchaseOrderCompletedIntegrationEvent message)
    {
        _logger.LogDebug(
            "Recording purchase orders completed metrics for PurchaseOrderId={PurchaseOrderId} CompletedAt={CompletedAt}",
            message.PurchaseOrderId,
            message.CompletedAt);

        var result = await _mediator.Send(
            new RecordPurchaseOrdersCompletedMetricsCommand
            {
                OccurredOn = message.CompletedAt
            },
            CancellationToken.None);

        if (result.IsFailure)
        {
            throw result.Exception!;
        }
    }
}