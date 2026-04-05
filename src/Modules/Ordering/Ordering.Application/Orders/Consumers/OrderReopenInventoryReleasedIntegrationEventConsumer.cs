using Invoria.Ordering.Application.Orders.Commands.CompleteReopenAfterInventoryReleased;
using Invoria.Ordering.Contracts.Events;
using MediatR;
using Rebus.Handlers;

namespace Invoria.Ordering.Application.Orders.Consumers;

public sealed class OrderReopenInventoryReleasedIntegrationEventConsumer
    : IHandleMessages<OrderReopenInventoryReleasedIntegrationEvent>
{
    private readonly IMediator _mediator;

    public OrderReopenInventoryReleasedIntegrationEventConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(OrderReopenInventoryReleasedIntegrationEvent message)
    {
        var result = await _mediator.Send(
            CompleteReopenAfterInventoryReleasedCommand.FromEvent(message),
            CancellationToken.None);

        if (!result.IsSuccess)
        {
            throw result.Exception ?? new InvalidOperationException("Complete reopen after inventory release failed.");
        }
    }
}
