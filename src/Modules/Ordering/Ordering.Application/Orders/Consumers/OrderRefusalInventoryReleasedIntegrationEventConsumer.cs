using Invoria.Ordering.Application.Orders.Commands.CompleteRefusalAfterInventoryReleased;
using Invoria.Ordering.Contracts.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Invoria.Ordering.Application.Orders.Consumers;

public sealed class OrderRefusalInventoryReleasedIntegrationEventConsumer
    : IHandleMessages<OrderRefusalInventoryReleasedIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrderRefusalInventoryReleasedIntegrationEventConsumer> _logger;

    public OrderRefusalInventoryReleasedIntegrationEventConsumer(
        IMediator mediator,
        ILogger<OrderRefusalInventoryReleasedIntegrationEventConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public Task Handle(OrderRefusalInventoryReleasedIntegrationEvent message)
    {
        _logger.LogDebug(
            "Consuming integration event {EventName} for OrderId={OrderId} OrderNumber={OrderNumber}",
            nameof(OrderRefusalInventoryReleasedIntegrationEvent),
            message.OrderId,
            message.OrderNumber);

        return _mediator.Send(
            CompleteRefusalAfterInventoryReleasedCommand.FromEvent(message),
            CancellationToken.None);
    }
}
