using Invoria.Ordering.Application.ReportOrderCompletedMetrics.Commands.RecordOrderCompletedMetrics;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Invoria.Ordering.Application.Orders.Sagas.Activities;

public sealed record RecordOrderCompletedMetricsSagaActivity(DateTimeOffset OccurredOn);

public sealed class RecordOrderCompletedMetricsSagaActivityHandler
    : IHandleMessages<RecordOrderCompletedMetricsSagaActivity>
{
    private readonly IMediator _mediator;
    private readonly ILogger<RecordOrderCompletedMetricsSagaActivityHandler> _logger;

    public RecordOrderCompletedMetricsSagaActivityHandler(
        IMediator mediator,
        ILogger<RecordOrderCompletedMetricsSagaActivityHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public Task Handle(RecordOrderCompletedMetricsSagaActivity message)
    {
        _logger.LogDebug(
            "Recording order completed metrics for OccurredOn={OccurredOn}",
            message.OccurredOn);

        return _mediator.Send(
            new RecordOrderCompletedMetricsCommand
            {
                OccurredOn = message.OccurredOn
            },
            CancellationToken.None);
    }
}