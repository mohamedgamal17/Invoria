namespace Invoria.Ordering.Contracts.Orders.Events;

/// <summary>
/// Published when inventory cannot complete allocation for an order (e.g. insufficient stock).
/// </summary>
public class OrderAllocationFailedIntegrationEvent
{
    public required string OrderId { get; set; }

    public required string Reason { get; set; }

    public string? CustomerId { get; set; }

    public string? OrderNumber { get; set; }

    public string? Details { get; set; }

    public List<OrderAllocationItemErrorModel> ItemErrors { get; set; } = [];
}

public class OrderAllocationItemErrorModel
{
    public required string OrderItemId { get; set; }

    public required string ProductId { get; set; }

    public required int RequestedQuantity { get; set; }

    public required int AvailableQuantity { get; set; }

    public required int Shortage { get; set; }
}
