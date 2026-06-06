using Invoria.Ordering.Application.Orders.Commands.ReviseOrder;
using Microsoft.Extensions.Logging;
using MediatR;
using Rebus.Handlers;

namespace Invoria.Ordering.Application.Orders.Sagas.Activities;

public sealed record ReviseOrderSagaActivity(string OrderId);

public sealed class ReviseOrderSagaActivityHandler
    : IHandleMessages<ReviseOrderSagaActivity>
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReviseOrderSagaActivityHandler> _logger;

    public ReviseOrderSagaActivityHandler(
        IMediator mediator,
        ILogger<ReviseOrderSagaActivityHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public Task Handle(ReviseOrderSagaActivity message)
    {
        _logger.LogDebug(
            "Revising order saga activity for OrderId={OrderId}",
            message.OrderId);

        return _mediator.Send(
            new ReviseOrderCommand(message.OrderId),
            CancellationToken.None);
    }
}
