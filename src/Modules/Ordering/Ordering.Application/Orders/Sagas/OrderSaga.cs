using Rebus.Sagas;

namespace Invoria.Ordering.Application.Orders.Sagas;

public sealed class OrderSaga : Saga<OrderSagaState>
{
    protected override void CorrelateMessages(ICorrelationConfig<OrderSagaState> config)
    {
        
    }
}
