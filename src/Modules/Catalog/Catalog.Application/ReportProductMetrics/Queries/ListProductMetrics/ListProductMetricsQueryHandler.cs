using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Catalog.Application.ReportProductMetrics.Factories;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Domain;
using Invoria.Catalog.Domain.Products;

namespace Invoria.Catalog.Application.ReportProductMetrics.Queries.ListProductMetrics
{
    using ReportProductMetricsEntity = Invoria.Catalog.Domain.Products.ReportProductMetrics;

    public class ListProductMetricsQueryHandler : IApplicatonRequestHandler<ListProductMetricsQuery, PagingDto<ReportProductMetricsPeriodDto>>
    {
        private readonly ICatalogRepository<ReportProductMetricsEntity> _reportProductMetricsRepository;
        private readonly IReportProductMetricsResponseFactory _reportProductMetricsResponseFactory;

        public ListProductMetricsQueryHandler(
            ICatalogRepository<ReportProductMetricsEntity> reportProductMetricsRepository,
            IReportProductMetricsResponseFactory reportProductMetricsResponseFactory)
        {
            _reportProductMetricsRepository = reportProductMetricsRepository;
            _reportProductMetricsResponseFactory = reportProductMetricsResponseFactory;
        }

        public async Task<Result<PagingDto<ReportProductMetricsPeriodDto>>> Handle(ListProductMetricsQuery request, CancellationToken cancellationToken)
        {
            var query = _reportProductMetricsRepository.AsQuerable();

            query = query.Where(x => x.Period == request.Period);

            query = query.OrderByDescending(x => x.Date);

            var result = await query.ToPaged(request.Skip, request.Length);

            var response = await _reportProductMetricsResponseFactory.PreparePagingDto(result);

            return response;
        }
    }
}
