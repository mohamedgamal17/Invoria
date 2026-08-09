using Invoria.BuildingBlocks.Domain.Enums;

namespace Invoria.Procurement.Contracts.Dtos;

public class ReportPurchaseSalesMetricsPeriodDto
{
    public DateTimeOffset Date { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public ReportPeriod Period { get; set; }
}