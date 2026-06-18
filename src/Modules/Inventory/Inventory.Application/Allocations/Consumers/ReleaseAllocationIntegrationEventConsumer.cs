using Invoria.Inventory.Application.Allocations.Commands.ReleaseAllocation;
using Invoria.Inventory.Contracts.Allocations.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Invoria.Inventory.Application.Allocations.Consumers;

public sealed class ReleaseAllocationIntegrationEventConsumer
    : IHandleMessages<ReleaseAllocationIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReleaseAllocationIntegrationEventConsumer> _logger;

    public ReleaseAllocationIntegrationEventConsumer(
        IMediator mediator,
        ILogger<ReleaseAllocationIntegrationEventConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public Task Handle(ReleaseAllocationIntegrationEvent message)
    {
        _logger.LogDebug(
            "Consuming integration event {EventName} for AllocationId={AllocationId}",
            nameof(ReleaseAllocationIntegrationEvent),
            message.AllocationId);

        return _mediator.Send(ReleaseAllocationCommand.FromEvent(message), CancellationToken.None);
    }
}
