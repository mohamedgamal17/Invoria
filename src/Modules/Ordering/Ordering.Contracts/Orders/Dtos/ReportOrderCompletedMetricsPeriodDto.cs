using Invoria.BuildingBlocks.Domain.Enums;

namespace Invoria.Ordering.Contracts.Orders.Dtos;

public class ReportOrderCompletedMetricsPeriodDto
{
    public DateTimeOffset Date { get; set; }
    public long TotalCount { get; set; }
    public ReportPeriod Period { get; set; }
}