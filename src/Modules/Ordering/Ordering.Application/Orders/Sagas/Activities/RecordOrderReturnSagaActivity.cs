using Invoria.Ordering.Application.Orders.Commands.RecordOrderReturn;
using Microsoft.Extensions.Logging;
using MediatR;
using Rebus.Handlers;

namespace Invoria.Ordering.Application.Orders.Sagas.Activities;

public sealed record RecordOrderReturnSagaActivity(string OrderId, string ReturnId);

public sealed class RecordOrderReturnSagaActivityHandler
    : IHandleMessages<RecordOrderReturnSagaActivity>
{
    private readonly IMediator _mediator;
    private readonly ILogger<RecordOrderReturnSagaActivityHandler> _logger;

    public RecordOrderReturnSagaActivityHandler(
        IMediator mediator,
        ILogger<RecordOrderReturnSagaActivityHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public Task Handle(RecordOrderReturnSagaActivity message)
    {
        _logger.LogDebug(
            "Recording order return saga activity for OrderId={OrderId} ReturnId={ReturnId}",
            message.OrderId,
            message.ReturnId);

        return _mediator.Send(
            new RecordOrderReturnCommand(message.OrderId, message.ReturnId),
            CancellationToken.None);
    }
}
