using Invoria.Inventory.Application.Batches.Commands.DispatchOrder;
using Invoria.Ordering.Contracts.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Invoria.Inventory.Application.Batches.Consumers;

public sealed class OrderDispatchedIntegrationEventConsumer : IHandleMessages<OrderDispatchedIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrderDispatchedIntegrationEventConsumer> _logger;

    public OrderDispatchedIntegrationEventConsumer(
        IMediator mediator,
        ILogger<OrderDispatchedIntegrationEventConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(OrderDispatchedIntegrationEvent message)
    {
        _logger.LogDebug(
            "Consuming integration event {EventName} for OrderId={OrderId} OrderNumber={OrderNumber}",
            nameof(OrderDispatchedIntegrationEvent),
            message.Id,
            message.OrderNumber);

        var result = await _mediator.Send(DispatchOrderCommand.FromEvent(message), CancellationToken.None);
        if (result.IsFailure)
        {
            throw result.Exception!;
        }
    }
}
