using Invoria.Inventory.Application.Allocations.Queries.GetOrderAllocationConsumption;
using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.Ordering.Contracts.Orders.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;

namespace Invoria.Inventory.Application.Allocations.Consumers;

public sealed class RequestOrderAllocationIntegrationEventConsumer
    : IHandleMessages<RequestOrderAllocationIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly IBus _bus;
    private readonly ILogger<RequestOrderAllocationIntegrationEventConsumer> _logger;

    public RequestOrderAllocationIntegrationEventConsumer(
        IMediator mediator,
        IBus bus,
        ILogger<RequestOrderAllocationIntegrationEventConsumer> logger)
    {
        _mediator = mediator;
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(RequestOrderAllocationIntegrationEvent message)
    {
        _logger.LogDebug(
            "Consuming integration event {EventName} for OrderId={OrderId} AllocationId={AllocationId}",
            nameof(RequestOrderAllocationIntegrationEvent),
            message.OrderId,
            message.AllocationId);

        var result = await _mediator.Send(
            new GetOrderAllocationConsumptionQuery { AllocationId = message.AllocationId },
            CancellationToken.None);

        if (result.IsFailure || result.Value is null)
        {
            throw new InvalidOperationException(
                $"Failed to build order allocation consumption for OrderId={message.OrderId} AllocationId={message.AllocationId}.");
        }

        _logger.LogDebug(
            "Publishing integration event {EventName} for OrderId={OrderId} AllocationId={AllocationId}",
            nameof(OrderAllocationConsumptionIntegrationEvent),
            result.Value.OrderId,
            result.Value.AllocationId);

        await _bus.Publish(result.Value);
    }
}
