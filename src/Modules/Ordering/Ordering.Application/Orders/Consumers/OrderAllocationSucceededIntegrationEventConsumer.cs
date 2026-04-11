using Invoria.Ordering.Application.Orders.Commands.RecordOrderAllocationSucceeded;
using Invoria.Ordering.Contracts.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Invoria.Ordering.Application.Orders.Consumers;

public sealed class OrderAllocationSucceededIntegrationEventConsumer : IHandleMessages<OrderAllocationSucceededIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrderAllocationSucceededIntegrationEventConsumer> _logger;

    public OrderAllocationSucceededIntegrationEventConsumer(
        IMediator mediator,
        ILogger<OrderAllocationSucceededIntegrationEventConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public Task Handle(OrderAllocationSucceededIntegrationEvent message)
    {
        _logger.LogDebug(
            "Consuming integration event {EventName} for OrderId={OrderId} OrderNumber={OrderNumber}",
            nameof(OrderAllocationSucceededIntegrationEvent),
            message.OrderId,
            message.OrderNumber);

        return _mediator.Send(RecordOrderAllocationSucceededCommand.FromEvent(message), CancellationToken.None);
    }
}
