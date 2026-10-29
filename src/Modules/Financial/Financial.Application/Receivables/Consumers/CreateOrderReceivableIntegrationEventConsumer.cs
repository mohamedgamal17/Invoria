using Invoria.Financial.Application.Receivables.Commands.CreateReceivable;
using Invoria.Financial.Contracts.Receivables.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Invoria.Financial.Application.Receivables.Consumers;

public class CreateOrderReceivableIntegrationEventConsumer
    : IHandleMessages<CreateOrderReceivableIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<CreateOrderReceivableIntegrationEventConsumer> _logger;

    public CreateOrderReceivableIntegrationEventConsumer(
        IMediator mediator,
        ILogger<CreateOrderReceivableIntegrationEventConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public Task Handle(CreateOrderReceivableIntegrationEvent message)
    {
        _logger.LogDebug(
            "Consuming integration event {EventName} for OrderId={OrderId} CustomerId={CustomerId}",
            nameof(CreateOrderReceivableIntegrationEvent),
            message.OrderId,
            message.CustomerId);

        var command = CreateReceivableCommand.FromEvent(message);

        return _mediator.Send(command, CancellationToken.None);
    }
}
