namespace Invoria.Ordering.Contracts.Orders.Events;

public class OrderAllocationSucceededIntegrationEvent
{
    public required string OrderId { get; set; }

    public string? OrderNumber { get; set; }

    public required string CustomerId { get; set; }

    public required DateTimeOffset AllocatedAt { get; set; }

    public required List<OrderAllocationSucceededLineModel> AllocatedLines { get; set; }
}

public class OrderAllocationSucceededLineModel
{
    public required string OrderItemId { get; set; }

    public required string ProductId { get; set; }

    public required int Quantity { get; set; }
}
