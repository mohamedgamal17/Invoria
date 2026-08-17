using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Catalog.Application.ReportProductMetrics.Factories;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Domain;
using Invoria.Catalog.Domain.Products;

namespace Invoria.Catalog.Application.ReportProductMetrics.Queries.GetProductMetrics
{
    using ReportProductMetricsEntity = Invoria.Catalog.Domain.Products.ReportProductMetrics;

    public class GetProductMetricsQueryHandler : IApplicatonRequestHandler<GetProductMetricsQuery, ReportProductMetricsDto>
    {
        private readonly ICatalogRepository<ReportProductMetricsEntity> _reportProductMetricsRepository;
        private readonly IReportProductMetricsResponseFactory _reportProductMetricsResponseFactory;

        public GetProductMetricsQueryHandler(
            ICatalogRepository<ReportProductMetricsEntity> reportProductMetricsRepository,
            IReportProductMetricsResponseFactory reportProductMetricsResponseFactory)
        {
            _reportProductMetricsRepository = reportProductMetricsRepository;
            _reportProductMetricsResponseFactory = reportProductMetricsResponseFactory;
        }

        public async Task<Result<ReportProductMetricsDto>> Handle(GetProductMetricsQuery request, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.Now;

            var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
            var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
            var yearStart = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, now.Offset);

            var daily = await _reportProductMetricsRepository.SingleOrDefault(
                x => x.Period == ReportPeriod.Daily && x.Date == todayStart, cancellationToken);
            var monthly = await _reportProductMetricsRepository.SingleOrDefault(
                x => x.Period == ReportPeriod.Monthly && x.Date == monthStart, cancellationToken);
            var yearly = await _reportProductMetricsRepository.SingleOrDefault(
                x => x.Period == ReportPeriod.Yearly && x.Date == yearStart, cancellationToken);
            var allTime = await _reportProductMetricsRepository.SingleOrDefault(
                x => x.Period == ReportPeriod.AllTheTime, cancellationToken);

            daily ??= new ReportProductMetricsEntity(todayStart, 0, ReportPeriod.Daily);
            monthly ??= new ReportProductMetricsEntity(monthStart, 0, ReportPeriod.Monthly);
            yearly ??= new ReportProductMetricsEntity(yearStart, 0, ReportPeriod.Yearly);
            allTime ??= new ReportProductMetricsEntity(DateTimeOffset.MinValue, 0, ReportPeriod.AllTheTime);

            var dto = await _reportProductMetricsResponseFactory.PrepareMetricsDto(daily, monthly, yearly, allTime);

            return dto;
        }
    }
}
