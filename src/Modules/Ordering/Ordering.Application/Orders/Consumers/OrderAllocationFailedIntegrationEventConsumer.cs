using Invoria.Ordering.Application.Orders.Commands.RecordOrderAllocationFailed;
using Invoria.Ordering.Contracts.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Invoria.Ordering.Application.Orders.Consumers;

public sealed class OrderAllocationFailedIntegrationEventConsumer : IHandleMessages<OrderAllocationFailedIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrderAllocationFailedIntegrationEventConsumer> _logger;

    public OrderAllocationFailedIntegrationEventConsumer(
        IMediator mediator,
        ILogger<OrderAllocationFailedIntegrationEventConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public Task Handle(OrderAllocationFailedIntegrationEvent message)
    {
        _logger.LogDebug(
            "Consuming integration event {EventName} for OrderId={OrderId} OrderNumber={OrderNumber}",
            nameof(OrderAllocationFailedIntegrationEvent),
            message.OrderId,
            message.OrderNumber);

        return _mediator.Send(RecordOrderAllocationFailedCommand.FromEvent(message), CancellationToken.None);
    }
}
