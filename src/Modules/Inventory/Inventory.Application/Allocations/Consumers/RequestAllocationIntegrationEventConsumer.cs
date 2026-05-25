using Invoria.Inventory.Application.Allocations.Commands.RequestAllocation;
using Invoria.Inventory.Contracts.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Invoria.Inventory.Application.Allocations.Consumers;

public sealed class RequestAllocationIntegrationEventConsumer
    : IHandleMessages<RequestAllocationIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<RequestAllocationIntegrationEventConsumer> _logger;

    public RequestAllocationIntegrationEventConsumer(
        IMediator mediator,
        ILogger<RequestAllocationIntegrationEventConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public Task Handle(RequestAllocationIntegrationEvent message)
    {
        _logger.LogDebug(
            "Consuming integration event {EventName} for AllocationId={AllocationId}",
            nameof(RequestAllocationIntegrationEvent),
            message.AllocationId);

        return _mediator.Send(RequestAllocationCommand.FromEvent(message), CancellationToken.None);
    }
}
