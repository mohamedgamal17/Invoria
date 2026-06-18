using Invoria.Inventory.Application.Returns.Commands.CreateImmediateReturn;
using Invoria.Inventory.Contracts.Returns.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Invoria.Inventory.Application.Returns.Consumers;

public class CreateImmediateReturnIntegrationEventConsumer
    : IHandleMessages<CreateImmediateReturnIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<CreateImmediateReturnIntegrationEventConsumer> _logger;

    public CreateImmediateReturnIntegrationEventConsumer(
        IMediator mediator,
        ILogger<CreateImmediateReturnIntegrationEventConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public Task Handle(CreateImmediateReturnIntegrationEvent message)
    {
        _logger.LogDebug(
            "Consuming integration event {EventName} for OrderId={OrderId} AllocationId={AllocationId}",
            nameof(CreateImmediateReturnIntegrationEvent),
            message.OrderId,
            message.AllocationId);

        return _mediator.Send(CreateImmediateReturnCommand.FromEvent(message), CancellationToken.None);
    }
}
