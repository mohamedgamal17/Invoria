using Invoria.Ordering.Contracts.Orders.Events;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Invoria.Ordering.Application.Orders.Sagas;

public sealed class OrderSaga : Saga<OrderSagaState>,
    IAmInitiatedBy<OrderCreatedIntegrationEvent>,
    IHandleMessages<OrderCreatedIntegrationEvent>
{
    protected override void CorrelateMessages(ICorrelationConfig<OrderSagaState> config)
    {
        config.Correlate<OrderCreatedIntegrationEvent>(m => m.Order.Id, d => d.OrderId);
    }

    public Task Handle(OrderCreatedIntegrationEvent message)
    {
        if (!IsNew)
        {
            return Task.CompletedTask;
        }

        Data.ApplyCreated(message.Order);

        return Task.CompletedTask;
    }
}
