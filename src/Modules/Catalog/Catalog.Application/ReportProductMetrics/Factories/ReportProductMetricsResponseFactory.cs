using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Domain.Products;

namespace Invoria.Catalog.Application.ReportProductMetrics.Factories
{
    using ReportProductMetricsEntity = Invoria.Catalog.Domain.Products.ReportProductMetrics;

    public class ReportProductMetricsResponseFactory : ResponseFactory<ReportProductMetricsEntity, ReportProductMetricsPeriodDto>, IReportProductMetricsResponseFactory
    {
        public override Task<ReportProductMetricsPeriodDto> PrepareDto(ReportProductMetricsEntity view)
        {
            var dto = new ReportProductMetricsPeriodDto
            {
                Date = view.Date,
                TotalCount = view.TotalCount,
                Period = view.Period
            };

            return Task.FromResult(dto);
        }

        public async Task<ReportProductMetricsDto> PrepareMetricsDto(
            ReportProductMetricsEntity daily,
            ReportProductMetricsEntity monthly,
            ReportProductMetricsEntity yearly,
            ReportProductMetricsEntity allTime)
        {
            var dto = new ReportProductMetricsDto
            {
                ThisDay = await PrepareDto(daily),
                ThisMonth = await PrepareDto(monthly),
                ThisYear = await PrepareDto(yearly),
                AllTime = await PrepareDto(allTime)
            };

            return dto;
        }
    }
}
