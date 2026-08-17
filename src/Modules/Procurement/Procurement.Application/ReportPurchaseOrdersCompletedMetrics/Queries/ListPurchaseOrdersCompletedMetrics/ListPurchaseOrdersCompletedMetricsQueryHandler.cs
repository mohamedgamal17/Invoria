using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Procurement.Application.ReportPurchaseOrdersCompletedMetrics.Factories;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Domain.PurchaseOrders;
using Invoria.Procurement.Domain.Repositories;

namespace Invoria.Procurement.Application.ReportPurchaseOrdersCompletedMetrics.Queries.ListPurchaseOrdersCompletedMetrics;

using ReportPurchaseOrdersCompletedMetricsEntity = Invoria.Procurement.Domain.PurchaseOrders.ReportPurchaseOrdersCompletedMetrics;

public class ListPurchaseOrdersCompletedMetricsQueryHandler : IApplicatonRequestHandler<ListPurchaseOrdersCompletedMetricsQuery, PagingDto<ReportPurchaseOrdersCompletedMetricsPeriodDto>>
{
    private readonly IProcurementRepository<ReportPurchaseOrdersCompletedMetricsEntity> _reportPurchaseOrdersCompletedMetricsRepository;
    private readonly IReportPurchaseOrdersCompletedMetricsResponseFactory _reportPurchaseOrdersCompletedMetricsResponseFactory;

    public ListPurchaseOrdersCompletedMetricsQueryHandler(
        IProcurementRepository<ReportPurchaseOrdersCompletedMetricsEntity> reportPurchaseOrdersCompletedMetricsRepository,
        IReportPurchaseOrdersCompletedMetricsResponseFactory reportPurchaseOrdersCompletedMetricsResponseFactory)
    {
        _reportPurchaseOrdersCompletedMetricsRepository = reportPurchaseOrdersCompletedMetricsRepository;
        _reportPurchaseOrdersCompletedMetricsResponseFactory = reportPurchaseOrdersCompletedMetricsResponseFactory;
    }

    public async Task<Result<PagingDto<ReportPurchaseOrdersCompletedMetricsPeriodDto>>> Handle(ListPurchaseOrdersCompletedMetricsQuery request, CancellationToken cancellationToken)
    {
        var query = _reportPurchaseOrdersCompletedMetricsRepository.AsQuerable();

        query = query.Where(x => x.Period == request.Period);

        query = query.OrderByDescending(x => x.Date);

        var result = await query.ToPaged(request.Skip, request.Length);

        var response = await _reportPurchaseOrdersCompletedMetricsResponseFactory.PreparePagingDto(result);

        return response;
    }
}