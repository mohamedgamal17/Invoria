using Invoria.Ordering.Contracts.Orders.Models;

namespace Invoria.Ordering.Contracts.Orders.Events;

/// <summary>
/// Published when a sales order is completed.
/// Consumed by OrderSaga to fan out return, invoice and receivable activities.
/// </summary>
public class OrderCompletedIntegrationEvent
{
    public required string OrderId { get; set; }

    public required string CustomerId { get; set; }

    public decimal Amount { get; set; }

    public required DateTimeOffset OccurredOn { get; set; }

    public string? AllocationId { get; set; }

    public List<OrderReturnLineModel> ReturnLines { get; set; } = [];

    public bool HasBillableItems { get; set; }
}
