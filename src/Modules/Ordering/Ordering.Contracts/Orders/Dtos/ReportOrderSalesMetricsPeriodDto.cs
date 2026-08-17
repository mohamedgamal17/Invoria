using Invoria.BuildingBlocks.Domain.Enums;

namespace Invoria.Ordering.Contracts.Orders.Dtos;

public class ReportOrderSalesMetricsPeriodDto
{
    public DateTimeOffset Date { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalNetAmount { get; set; }
    public decimal TotalReturnAmount { get; set; }
    public ReportPeriod Period { get; set; }
}