using Invoria.Inventory.Application.Batches.Commands.ReleaseOrderAllocations;
using Invoria.Ordering.Contracts.Events;
using MediatR;
using Rebus.Bus;
using Rebus.Handlers;

namespace Invoria.Inventory.Application.Batches.Consumers;

public sealed class ReleaseOrderAllocationsIntegrationEventConsumer
    : IHandleMessages<ReleaseOrderAllocationsIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly IBus _bus;

    public ReleaseOrderAllocationsIntegrationEventConsumer(IMediator mediator, IBus bus)
    {
        _mediator = mediator;
        _bus = bus;
    }

    public async Task Handle(ReleaseOrderAllocationsIntegrationEvent message)
    {
        var result = await _mediator.Send(
            ReleaseOrderAllocationsCommand.FromEvent(message),
            CancellationToken.None);

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(
                result.Exception?.Message ?? "Release order allocations failed.");
        }

        if (message.ReleaseReason == AllocationReleaseReason.Refusal)
        {
            await _bus.Publish(new OrderRefusalInventoryReleasedIntegrationEvent
            {
                OrderId = message.Id,
                OrderNumber = message.OrderNumber,
                CustomerId = message.CustomerId,
                ReleasedAt = DateTimeOffset.UtcNow
            });
        }
        else
        {
            await _bus.Publish(new OrderReopenInventoryReleasedIntegrationEvent
            {
                OrderId = message.Id,
                OrderNumber = message.OrderNumber,
                CustomerId = message.CustomerId,
                ReleasedAt = DateTimeOffset.UtcNow
            });
        }
    }
}
