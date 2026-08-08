using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Ordering.Application.ReportOrderSalesMetrics.Factories;
using Invoria.Ordering.Contracts.Orders.Dtos;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.ReportOrderSalesMetrics.Queries.ListOrderSalesMetrics;

using ReportOrderSalesMetricsEntity = Invoria.Ordering.Domain.Orders.ReportOrderSalesMetrics;

public class ListOrderSalesMetricsQueryHandler : IApplicatonRequestHandler<ListOrderSalesMetricsQuery, PagingDto<ReportOrderSalesMetricsPeriodDto>>
{
    private readonly IOrderingRepository<ReportOrderSalesMetricsEntity> _reportOrderSalesMetricsRepository;
    private readonly IReportOrderSalesMetricsResponseFactory _reportOrderSalesMetricsResponseFactory;

    public ListOrderSalesMetricsQueryHandler(
        IOrderingRepository<ReportOrderSalesMetricsEntity> reportOrderSalesMetricsRepository,
        IReportOrderSalesMetricsResponseFactory reportOrderSalesMetricsResponseFactory)
    {
        _reportOrderSalesMetricsRepository = reportOrderSalesMetricsRepository;
        _reportOrderSalesMetricsResponseFactory = reportOrderSalesMetricsResponseFactory;
    }

    public async Task<Result<PagingDto<ReportOrderSalesMetricsPeriodDto>>> Handle(ListOrderSalesMetricsQuery request, CancellationToken cancellationToken)
    {
        var query = _reportOrderSalesMetricsRepository.AsQuerable();

        query = query.Where(x => x.Period == request.Period);

        query = query.OrderByDescending(x => x.Date);

        var result = await query.ToPaged(request.Skip, request.Length);

        var response = await _reportOrderSalesMetricsResponseFactory.PreparePagingDto(result);

        return response;
    }
}