using Invoria.Ordering.Contracts.Orders.Models;

namespace Invoria.Ordering.Contracts.Orders.Events;

public class OrderReturnRequestedIntegrationEvent
{
    public string OrderId { get; set; } = null!;

    public string AllocationId { get; set; } = null!;

    public List<OrderReturnLineModel> Lines { get; set; } = [];
}
