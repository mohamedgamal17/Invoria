using Invoria.Inventory.Application.Returns.Commands.ProcessImmediateReturn;
using Invoria.Inventory.Contracts.Returns.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Invoria.Inventory.Application.Returns.Consumers;

public class ProcessImmediateReturnIntegrationEventConsumer
    : IHandleMessages<ProcessImmediateReturnIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProcessImmediateReturnIntegrationEventConsumer> _logger;

    public ProcessImmediateReturnIntegrationEventConsumer(
        IMediator mediator,
        ILogger<ProcessImmediateReturnIntegrationEventConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public Task Handle(ProcessImmediateReturnIntegrationEvent message)
    {
        _logger.LogDebug(
            "Consuming integration event {EventName} for ReturnId={ReturnId}",
            nameof(ProcessImmediateReturnIntegrationEvent),
            message.ReturnId);

        return _mediator.Send(ProcessImmediateReturnCommand.FromEvent(message), CancellationToken.None);
    }
}
