using Rebus.Sagas;

namespace Invoria.Ordering.Application.Orders.Sagas;

public sealed class OrderReturnSagaState : ISagaData
{
    public Guid Id { get; set; }

    public int Revision { get; set; }

    public string OrderId { get; set; } = default!;

    public string? ReturnId { get; set; }

    public string State { get; set; } = OrderReturnSagaProcessState.Requested;

    public void ApplyRequested(string orderId)
    {
        OrderId = orderId;
        State = OrderReturnSagaProcessState.Requested;
    }

    public void ApplyCompleted(string returnId)
    {
        ReturnId = returnId;
        State = OrderReturnSagaProcessState.Completed;
    }
}
