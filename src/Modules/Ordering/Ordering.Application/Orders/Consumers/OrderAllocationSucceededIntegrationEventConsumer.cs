using Invoria.Ordering.Application.Orders.Commands.RecordOrderAllocationSucceeded;
using Invoria.Ordering.Contracts.Events;
using MediatR;
using Rebus.Handlers;

namespace Invoria.Ordering.Application.Orders.Consumers;

public sealed class OrderAllocationSucceededIntegrationEventConsumer : IHandleMessages<OrderAllocationSucceededIntegrationEvent>
{
    private readonly IMediator _mediator;

    public OrderAllocationSucceededIntegrationEventConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task Handle(OrderAllocationSucceededIntegrationEvent message) =>
        _mediator.Send(RecordOrderAllocationSucceededCommand.FromEvent(message), CancellationToken.None);
}
