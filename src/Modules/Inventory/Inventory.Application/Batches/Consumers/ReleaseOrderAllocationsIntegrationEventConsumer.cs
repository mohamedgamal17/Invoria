using Invoria.Inventory.Application.Batches.Commands.ReleaseOrderAllocations;
using Invoria.Ordering.Contracts.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;

namespace Invoria.Inventory.Application.Batches.Consumers;

public sealed class ReleaseOrderAllocationsIntegrationEventConsumer
    : IHandleMessages<ReleaseOrderAllocationsIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly IBus _bus;
    private readonly ILogger<ReleaseOrderAllocationsIntegrationEventConsumer> _logger;

    public ReleaseOrderAllocationsIntegrationEventConsumer(
        IMediator mediator,
        IBus bus,
        ILogger<ReleaseOrderAllocationsIntegrationEventConsumer> logger)
    {
        _mediator = mediator;
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(ReleaseOrderAllocationsIntegrationEvent message)
    {
        _logger.LogDebug(
            "Consuming integration event {EventName} for OrderId={OrderId} OrderNumber={OrderNumber}",
            nameof(ReleaseOrderAllocationsIntegrationEvent),
            message.Id,
            message.OrderNumber);

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
            _logger.LogDebug(
                "Publishing integration event {EventName} for OrderId={OrderId} OrderNumber={OrderNumber}",
                nameof(OrderRefusalInventoryReleasedIntegrationEvent),
                message.Id,
                message.OrderNumber);
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
            _logger.LogDebug(
                "Publishing integration event {EventName} for OrderId={OrderId} OrderNumber={OrderNumber}",
                nameof(OrderReopenInventoryReleasedIntegrationEvent),
                message.Id,
                message.OrderNumber);
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
