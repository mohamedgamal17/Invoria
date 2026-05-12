using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Reporting.Domain.Orders;

public class ReportedOrderLine : IBaseEntity
{
    public string Id { get; set; } = null!;
    public string ReportedOrderId { get; set; } = null!;
    public string ProductId { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }

    public ReportedOrder? ReportedOrder { get; set; }
}
