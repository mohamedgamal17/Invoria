using Invoria.Ordering.Contracts.Orders.Events;
using Invoria.Ordering.Contracts.Orders.Models;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;

namespace Invoria.Ordering.Application.Orders.Sagas.Activities;

public sealed record CreateOrderReturnSagaActivity(
    string OrderId,
    string AllocationId,
    List<OrderReturnLineModel> Lines);

public sealed class CreateOrderReturnSagaActivityHandler
    : IHandleMessages<CreateOrderReturnSagaActivity>
{
    private readonly IBus _bus;
    private readonly ILogger<CreateOrderReturnSagaActivityHandler> _logger;

    public CreateOrderReturnSagaActivityHandler(
        IBus bus,
        ILogger<CreateOrderReturnSagaActivityHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public Task Handle(CreateOrderReturnSagaActivity message)
    {
        _logger.LogDebug(
            "Creating order return saga activity for OrderId={OrderId} AllocationId={AllocationId}",
            message.OrderId,
            message.AllocationId);

        return _bus.Publish(new OrderReturnRequestedIntegrationEvent
        {
            OrderId = message.OrderId,
            AllocationId = message.AllocationId,
            Lines = message.Lines
        });
    }
}
