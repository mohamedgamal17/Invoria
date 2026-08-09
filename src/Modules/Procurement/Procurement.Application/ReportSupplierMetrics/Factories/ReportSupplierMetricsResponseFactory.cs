using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Domain.Parties;

namespace Invoria.Procurement.Application.ReportSupplierMetrics.Factories;

using ReportSupplierMetricsEntity = Invoria.Procurement.Domain.Parties.ReportSupplierMetrics;

public class ReportSupplierMetricsResponseFactory : ResponseFactory<ReportSupplierMetricsEntity, ReportSupplierMetricsPeriodDto>, IReportSupplierMetricsResponseFactory
{
    public override Task<ReportSupplierMetricsPeriodDto> PrepareDto(ReportSupplierMetricsEntity view)
    {
        var dto = new ReportSupplierMetricsPeriodDto
        {
            Date = view.Date,
            TotalCount = view.TotalCount,
            Period = view.Period
        };

        return Task.FromResult(dto);
    }

    public async Task<ReportSupplierMetricsDto> PrepareMetricsDto(
        ReportSupplierMetricsEntity daily,
        ReportSupplierMetricsEntity monthly,
        ReportSupplierMetricsEntity yearly,
        ReportSupplierMetricsEntity allTime)
    {
        var dto = new ReportSupplierMetricsDto
        {
            ThisDay = await PrepareDto(daily),
            ThisMonth = await PrepareDto(monthly),
            ThisYear = await PrepareDto(yearly),
            AllTime = await PrepareDto(allTime)
        };

        return dto;
    }
}