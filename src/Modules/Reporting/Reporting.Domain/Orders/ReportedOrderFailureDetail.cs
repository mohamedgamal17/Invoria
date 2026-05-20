using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Reporting.Domain.Orders;

public class ReportedOrderFailureDetail : IBaseEntity
{
    public string Id { get; set; } = null!;
    public string ReportedOrderId { get; set; } = null!;
    public string ItemId { get; set; } = null!;
    public int QuantityRequested { get; set; }
    public int QuantityAvailable { get; set; }
    public int Shortage { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    public ReportedOrder? ReportedOrder { get; set; }
}
