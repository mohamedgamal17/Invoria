using Invoria.Inventory.Application.Allocations.Commands.CompleteAllocation;
using Invoria.Ordering.Contracts.Orders.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Invoria.Inventory.Application.Allocations.Consumers;

public sealed class OrderCompletedIntegrationEventConsumer
    : IHandleMessages<OrderCompletedIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrderCompletedIntegrationEventConsumer> _logger;

    public OrderCompletedIntegrationEventConsumer(
        IMediator mediator,
        ILogger<OrderCompletedIntegrationEventConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public Task Handle(OrderCompletedIntegrationEvent message)
    {
        _logger.LogDebug(
            "Consuming integration event {EventName} for OrderId={OrderId} AllocationId={AllocationId}",
            nameof(OrderCompletedIntegrationEvent),
            message.OrderId,
            message.AllocationId);

        return _mediator.Send(CompleteAllocationCommand.FromEvent(message), CancellationToken.None);
    }
}
