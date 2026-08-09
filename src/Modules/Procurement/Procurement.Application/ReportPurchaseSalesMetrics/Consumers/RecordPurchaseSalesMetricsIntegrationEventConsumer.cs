using Invoria.Procurement.Application.ReportPurchaseSalesMetrics.Commands.RecordPurchaseSalesMetrics;
using Invoria.Procurement.Contracts.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Invoria.Procurement.Application.ReportPurchaseSalesMetrics.Consumers;

public sealed class RecordPurchaseSalesMetricsIntegrationEventConsumer
    : IHandleMessages<PurchaseOrderCompletedIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<RecordPurchaseSalesMetricsIntegrationEventConsumer> _logger;

    public RecordPurchaseSalesMetricsIntegrationEventConsumer(
        IMediator mediator,
        ILogger<RecordPurchaseSalesMetricsIntegrationEventConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(PurchaseOrderCompletedIntegrationEvent message)
    {
        _logger.LogDebug(
            "Recording purchase sales metrics for PurchaseOrderId={PurchaseOrderId} CompletedAt={CompletedAt}",
            message.PurchaseOrderId,
            message.CompletedAt);

        var result = await _mediator.Send(
            new RecordPurchaseSalesMetricsCommand
            {
                PurchaseOrderId = message.PurchaseOrderId,
                OccurredOn = message.CompletedAt
            },
            CancellationToken.None);

        if (result.IsFailure)
        {
            throw result.Exception!;
        }
    }
}