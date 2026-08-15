using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.Ordering.Application.OrderAllocationConsumptions.Commands.CreateOrderAllocationConsumption;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Invoria.Ordering.Application.OrderAllocationConsumptions.Consumers;

public sealed class OrderAllocationConsumptionIntegrationEventConsumer
    : IHandleMessages<OrderAllocationConsumptionIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrderAllocationConsumptionIntegrationEventConsumer> _logger;

    public OrderAllocationConsumptionIntegrationEventConsumer(
        IMediator mediator,
        ILogger<OrderAllocationConsumptionIntegrationEventConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(OrderAllocationConsumptionIntegrationEvent message)
    {
        _logger.LogDebug(
            "Consuming integration event {EventName} for OrderId={OrderId} AllocationId={AllocationId}",
            nameof(OrderAllocationConsumptionIntegrationEvent),
            message.OrderId,
            message.AllocationId);

        var result = await _mediator.Send(
            CreateOrderAllocationConsumptionCommand.FromEvent(message),
            CancellationToken.None);

        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                $"Failed to create order allocation consumption for OrderId={message.OrderId} AllocationId={message.AllocationId}.");
        }
    }
}
