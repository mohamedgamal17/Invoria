using Invoria.Ordering.Application.ReportOrderSalesMetrics.Commands.RecordOrderSalesMetrics;
using Microsoft.Extensions.Logging;
using MediatR;
using Rebus.Handlers;

namespace Invoria.Ordering.Application.Orders.Sagas.Activities;

public sealed record RecordOrderSalesSagaActivity(string OrderId, DateTimeOffset OccurredOn);

public sealed class RecordOrderSalesSagaActivityHandler
    : IHandleMessages<RecordOrderSalesSagaActivity>
{
    private readonly IMediator _mediator;
    private readonly ILogger<RecordOrderSalesSagaActivityHandler> _logger;

    public RecordOrderSalesSagaActivityHandler(
        IMediator mediator,
        ILogger<RecordOrderSalesSagaActivityHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public Task Handle(RecordOrderSalesSagaActivity message)
    {
        _logger.LogDebug(
            "Recording order sales saga activity for OrderId={OrderId}",
            message.OrderId);

        return _mediator.Send(
            new RecordOrderSalesMetricsCommand
            {
                OrderId = message.OrderId,
                OccurredOn = message.OccurredOn
            },
            CancellationToken.None);
    }
}