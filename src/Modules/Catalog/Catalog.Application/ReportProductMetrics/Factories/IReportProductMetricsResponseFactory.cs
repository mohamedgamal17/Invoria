using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Domain.Products;

namespace Invoria.Catalog.Application.ReportProductMetrics.Factories
{
    using ReportProductMetricsEntity = Invoria.Catalog.Domain.Products.ReportProductMetrics;

    public interface IReportProductMetricsResponseFactory : IResponseFactory<ReportProductMetricsEntity, ReportProductMetricsPeriodDto>
    {
        Task<ReportProductMetricsDto> PrepareMetricsDto(
            ReportProductMetricsEntity daily,
            ReportProductMetricsEntity monthly,
            ReportProductMetricsEntity yearly,
            ReportProductMetricsEntity allTime);
    }
}
