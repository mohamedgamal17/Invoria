using Invoria.Ordering.Application.Orders.Commands.MarkOrderAllocated;
using Microsoft.Extensions.Logging;
using MediatR;
using Rebus.Handlers;

namespace Invoria.Ordering.Application.Orders.Sagas.Activities;

public sealed record MarkOrderAllocatedSagaActivity(string OrderId);

public sealed class MarkOrderAllocatedSagaActivityHandler
    : IHandleMessages<MarkOrderAllocatedSagaActivity>
{
    private readonly IMediator _mediator;
    private readonly ILogger<MarkOrderAllocatedSagaActivityHandler> _logger;

    public MarkOrderAllocatedSagaActivityHandler(
        IMediator mediator,
        ILogger<MarkOrderAllocatedSagaActivityHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public Task Handle(MarkOrderAllocatedSagaActivity message)
    {
        _logger.LogDebug(
            "Marking order allocated saga activity for OrderId={OrderId}",
            message.OrderId);

        return _mediator.Send(
            new MarkOrderAllocatedCommand(message.OrderId),
            CancellationToken.None);
    }
}
