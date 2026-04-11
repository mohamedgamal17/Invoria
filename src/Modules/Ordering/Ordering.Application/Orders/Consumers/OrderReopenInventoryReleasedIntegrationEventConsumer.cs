using Invoria.Ordering.Application.Orders.Commands.CompleteReopenAfterInventoryReleased;
using Invoria.Ordering.Contracts.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Invoria.Ordering.Application.Orders.Consumers;

public sealed class OrderReopenInventoryReleasedIntegrationEventConsumer
    : IHandleMessages<OrderReopenInventoryReleasedIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrderReopenInventoryReleasedIntegrationEventConsumer> _logger;

    public OrderReopenInventoryReleasedIntegrationEventConsumer(
        IMediator mediator,
        ILogger<OrderReopenInventoryReleasedIntegrationEventConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(OrderReopenInventoryReleasedIntegrationEvent message)
    {
        _logger.LogDebug(
            "Consuming integration event {EventName} for OrderId={OrderId} OrderNumber={OrderNumber}",
            nameof(OrderReopenInventoryReleasedIntegrationEvent),
            message.OrderId,
            message.OrderNumber);

        var result = await _mediator.Send(
            CompleteReopenAfterInventoryReleasedCommand.FromEvent(message),
            CancellationToken.None);

        if (!result.IsSuccess)
        {
            throw result.Exception ?? new InvalidOperationException("Complete reopen after inventory release failed.");
        }
    }
}
