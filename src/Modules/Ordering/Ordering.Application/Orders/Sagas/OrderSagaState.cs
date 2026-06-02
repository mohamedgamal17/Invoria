using Rebus.Sagas;

namespace Invoria.Ordering.Application.Orders.Sagas;

public sealed class OrderSagaState : ISagaData
{
    public Guid Id { get; set; }

    public int Revision { get; set; }

    public string OrderId { get; set; } = default!;

    public string OrderNumber { get; set; } = default!;

    public string CustomerId { get; set; } = default!;

    public string State { get; set; } = OrderSagaProcessState.Created;
}
