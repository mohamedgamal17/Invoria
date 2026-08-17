using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Domain.Parties;

namespace Invoria.Procurement.Application.ReportSupplierMetrics.Factories;

using ReportSupplierMetricsEntity = Invoria.Procurement.Domain.Parties.ReportSupplierMetrics;

public interface IReportSupplierMetricsResponseFactory : IResponseFactory<ReportSupplierMetricsEntity, ReportSupplierMetricsPeriodDto>
{
    Task<ReportSupplierMetricsDto> PrepareMetricsDto(
        ReportSupplierMetricsEntity daily,
        ReportSupplierMetricsEntity monthly,
        ReportSupplierMetricsEntity yearly,
        ReportSupplierMetricsEntity allTime);
}