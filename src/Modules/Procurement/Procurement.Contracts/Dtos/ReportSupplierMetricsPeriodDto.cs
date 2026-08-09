using Invoria.BuildingBlocks.Domain.Enums;

namespace Invoria.Procurement.Contracts.Dtos;

public class ReportSupplierMetricsPeriodDto
{
    public DateTimeOffset Date { get; set; }
    public long TotalCount { get; set; }
    public ReportPeriod Period { get; set; }
}