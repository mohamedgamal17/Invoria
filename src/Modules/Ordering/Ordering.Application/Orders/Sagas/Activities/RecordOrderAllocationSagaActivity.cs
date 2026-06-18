using Invoria.Ordering.Application.Orders.Commands.RecordOrderAllocation;
using Microsoft.Extensions.Logging;
using MediatR;
using Rebus.Handlers;

namespace Invoria.Ordering.Application.Orders.Sagas.Activities;

public sealed record RecordOrderAllocationSagaActivity(string OrderId, string AllocationId);

public sealed class RecordOrderAllocationSagaActivityHandler
    : IHandleMessages<RecordOrderAllocationSagaActivity>
{
    private readonly IMediator _mediator;
    private readonly ILogger<RecordOrderAllocationSagaActivityHandler> _logger;

    public RecordOrderAllocationSagaActivityHandler(
        IMediator mediator,
        ILogger<RecordOrderAllocationSagaActivityHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public Task Handle(RecordOrderAllocationSagaActivity message)
    {
        _logger.LogDebug(
            "Recording order allocation saga activity for OrderId={OrderId} AllocationId={AllocationId}",
            message.OrderId,
            message.AllocationId);

        return _mediator.Send(
            new RecordOrderAllocationCommand(message.OrderId, message.AllocationId),
            CancellationToken.None);
    }
}
