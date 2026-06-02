using Invoria.Inventory.Application.Batches.Commands.AllocateOrder;
using Invoria.Ordering.Contracts.Orders.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Invoria.Inventory.Application.Batches.Consumers;

public sealed class AllocateOrderIntegrationEventConsumer : IHandleMessages<AllocateOrderIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<AllocateOrderIntegrationEventConsumer> _logger;

    public AllocateOrderIntegrationEventConsumer(
        IMediator mediator,
        ILogger<AllocateOrderIntegrationEventConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public Task Handle(AllocateOrderIntegrationEvent message)
    {
        _logger.LogDebug(
            "Consuming integration event {EventName} for OrderId={OrderId} OrderNumber={OrderNumber}",
            nameof(AllocateOrderIntegrationEvent),
            message.Id,
            message.OrderNumber);

        return _mediator.Send(AllocateOrderCommand.FromEvent(message), CancellationToken.None);
    }
}
