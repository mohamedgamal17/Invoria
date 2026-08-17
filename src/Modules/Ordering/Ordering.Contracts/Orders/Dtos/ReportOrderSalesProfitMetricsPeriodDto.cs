using Invoria.BuildingBlocks.Domain.Enums;

namespace Invoria.Ordering.Contracts.Orders.Dtos;

public class ReportOrderSalesProfitMetricsPeriodDto
{
    public DateTimeOffset Date { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalProfit { get; set; }
    public decimal TotalReturnAmount { get; set; }
    public ReportPeriod Period { get; set; }
}