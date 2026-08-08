using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Ordering.Application.ReportOrderCompletedMetrics.Factories;
using Invoria.Ordering.Contracts.Orders.Dtos;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.ReportOrderCompletedMetrics.Queries.ListOrderCompletedMetrics;

using ReportOrderCompletedMetricsEntity = Invoria.Ordering.Domain.Orders.ReportOrderCompletedMetrics;

public class ListOrderCompletedMetricsQueryHandler : IApplicatonRequestHandler<ListOrderCompletedMetricsQuery, PagingDto<ReportOrderCompletedMetricsPeriodDto>>
{
    private readonly IOrderingRepository<ReportOrderCompletedMetricsEntity> _reportOrderCompletedMetricsRepository;
    private readonly IReportOrderCompletedMetricsResponseFactory _reportOrderCompletedMetricsResponseFactory;

    public ListOrderCompletedMetricsQueryHandler(
        IOrderingRepository<ReportOrderCompletedMetricsEntity> reportOrderCompletedMetricsRepository,
        IReportOrderCompletedMetricsResponseFactory reportOrderCompletedMetricsResponseFactory)
    {
        _reportOrderCompletedMetricsRepository = reportOrderCompletedMetricsRepository;
        _reportOrderCompletedMetricsResponseFactory = reportOrderCompletedMetricsResponseFactory;
    }

    public async Task<Result<PagingDto<ReportOrderCompletedMetricsPeriodDto>>> Handle(ListOrderCompletedMetricsQuery request, CancellationToken cancellationToken)
    {
        var query = _reportOrderCompletedMetricsRepository.AsQuerable();

        query = query.Where(x => x.Period == request.Period);

        query = query.OrderByDescending(x => x.Date);

        var result = await query.ToPaged(request.Skip, request.Length);

        var response = await _reportOrderCompletedMetricsResponseFactory.PreparePagingDto(result);

        return response;
    }
}