using Invoria.Inventory.Application.Allocations.Commands.RequestAllocation;
using Invoria.Inventory.Contracts.Allocations.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Invoria.Inventory.Application.Allocations.Consumers;

public sealed class AllocationCreatedIntegrationEventConsumer
    : IHandleMessages<AllocationCreatedIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<AllocationCreatedIntegrationEventConsumer> _logger;

    public AllocationCreatedIntegrationEventConsumer(
        IMediator mediator,
        ILogger<AllocationCreatedIntegrationEventConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public Task Handle(AllocationCreatedIntegrationEvent message)
    {
        _logger.LogDebug(
            "Consuming integration event {EventName} for AllocationId={AllocationId} OrderId={OrderId}",
            nameof(AllocationCreatedIntegrationEvent),
            message.Allocation.Id,
            message.Allocation.OrderId);

        return _mediator.Send(
            new RequestAllocationCommand { AllocationId = message.Allocation.Id },
            CancellationToken.None);
    }
}
