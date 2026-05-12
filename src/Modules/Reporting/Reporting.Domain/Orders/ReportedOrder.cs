using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Contracts.Orders;

namespace Invoria.Reporting.Domain.Orders;

/// <summary>
/// Read-side projection row for an order. Plain data carrier for persistence and queries; no invariants.
/// </summary>
public class ReportedOrder : IBaseEntity
{
    public string Id { get; set; } = null!;
    public string OrderNumber { get; set; } = null!;
    public string CustomerId { get; set; } = null!;
    public OrderStatus OrderStatus { get; set; }
    public FullfillmentStatus FullfillmentStatus { get; set; }
    public OrderPaymentType PaymentType { get; set; }
    public OrderPaymentStatus PaymentStatus { get; set; }
    public decimal TotalOrderAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal AmountOutstanding { get; set; }
    public long ReplicationVersion { get; set; }
    public DateTimeOffset? SourceLastKnownAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    public List<ReportedOrderLine> Lines { get; set; } = new();
    public List<ReportedOrderPayment> Payments { get; set; } = new();
    public List<ReportedOrderStateTransition> StateTransitions { get; set; } = new();
    public List<ReportedOrderFailureDetail> FailureDetails { get; set; } = new();
}
