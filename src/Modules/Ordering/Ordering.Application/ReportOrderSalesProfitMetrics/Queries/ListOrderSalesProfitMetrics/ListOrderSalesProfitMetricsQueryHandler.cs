using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Ordering.Application.ReportOrderSalesProfitMetrics.Factories;
using Invoria.Ordering.Contracts.Orders.Dtos;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.ReportOrderSalesProfitMetrics.Queries.ListOrderSalesProfitMetrics;

using ReportOrderSalesProfitMetricsEntity = Invoria.Ordering.Domain.Orders.ReportOrderSalesProfitMetrics;

public class ListOrderSalesProfitMetricsQueryHandler : IApplicatonRequestHandler<ListOrderSalesProfitMetricsQuery, PagingDto<ReportOrderSalesProfitMetricsPeriodDto>>
{
    private readonly IOrderingRepository<ReportOrderSalesProfitMetricsEntity> _reportOrderSalesProfitMetricsRepository;
    private readonly IReportOrderSalesProfitMetricsResponseFactory _reportOrderSalesProfitMetricsResponseFactory;

    public ListOrderSalesProfitMetricsQueryHandler(
        IOrderingRepository<ReportOrderSalesProfitMetricsEntity> reportOrderSalesProfitMetricsRepository,
        IReportOrderSalesProfitMetricsResponseFactory reportOrderSalesProfitMetricsResponseFactory)
    {
        _reportOrderSalesProfitMetricsRepository = reportOrderSalesProfitMetricsRepository;
        _reportOrderSalesProfitMetricsResponseFactory = reportOrderSalesProfitMetricsResponseFactory;
    }

    public async Task<Result<PagingDto<ReportOrderSalesProfitMetricsPeriodDto>>> Handle(ListOrderSalesProfitMetricsQuery request, CancellationToken cancellationToken)
    {
        var query = _reportOrderSalesProfitMetricsRepository.AsQuerable();

        query = query.Where(x => x.Period == request.Period);

        query = query.OrderByDescending(x => x.Date);

        var result = await query.ToPaged(request.Skip, request.Length);

        var response = await _reportOrderSalesProfitMetricsResponseFactory.PreparePagingDto(result);

        return response;
    }
}