using Invoria.Inventory.Contracts.Returns.Events;
using Invoria.Ordering.Application.Orders.Extensions;
using Invoria.Ordering.Application.Orders.Sagas.Activities;
using Invoria.Ordering.Contracts.Orders.Events;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Invoria.Ordering.Application.Orders.Sagas;

public sealed class OrderReturnSaga : Saga<OrderReturnSagaState>,
    IAmInitiatedBy<OrderReturnRequestedIntegrationEvent>,
    IHandleMessages<OrderReturnRequestedIntegrationEvent>,
    IHandleMessages<ImmediateReturnCreatedIntegrationEvent>
{
    private readonly IBus _bus;

    public OrderReturnSaga(IBus bus)
    {
        _bus = bus;
    }

    protected override void CorrelateMessages(ICorrelationConfig<OrderReturnSagaState> config)
    {
        config.Correlate<OrderReturnRequestedIntegrationEvent>(m => m.OrderId, d => d.OrderId);
        config.Correlate<ImmediateReturnCreatedIntegrationEvent>(m => m.OrderId, d => d.OrderId);
    }

    public async Task Handle(OrderReturnRequestedIntegrationEvent message)
    {
        Data.ApplyRequested(message.OrderId);

        await _bus.Publish(message.ToCreateImmediateReturnIntegrationEvent());
    }

    public async Task Handle(ImmediateReturnCreatedIntegrationEvent message)
    {
        Data.ApplyCompleted(message.ReturnId);

        await _bus.Publish(new RecordOrderReturnSagaActivity(message.OrderId, message.ReturnId));
    }
}
