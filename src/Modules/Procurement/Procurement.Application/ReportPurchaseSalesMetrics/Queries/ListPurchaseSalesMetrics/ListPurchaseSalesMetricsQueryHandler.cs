using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Procurement.Application.ReportPurchaseSalesMetrics.Factories;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Domain.PurchaseOrders;
using Invoria.Procurement.Domain.Repositories;

namespace Invoria.Procurement.Application.ReportPurchaseSalesMetrics.Queries.ListPurchaseSalesMetrics;

using ReportPurchaseSalesMetricsEntity = Invoria.Procurement.Domain.PurchaseOrders.ReportPurchaseSalesMetrics;

public class ListPurchaseSalesMetricsQueryHandler : IApplicatonRequestHandler<ListPurchaseSalesMetricsQuery, PagingDto<ReportPurchaseSalesMetricsPeriodDto>>
{
    private readonly IProcurementRepository<ReportPurchaseSalesMetricsEntity> _reportPurchaseSalesMetricsRepository;
    private readonly IReportPurchaseSalesMetricsResponseFactory _reportPurchaseSalesMetricsResponseFactory;

    public ListPurchaseSalesMetricsQueryHandler(
        IProcurementRepository<ReportPurchaseSalesMetricsEntity> reportPurchaseSalesMetricsRepository,
        IReportPurchaseSalesMetricsResponseFactory reportPurchaseSalesMetricsResponseFactory)
    {
        _reportPurchaseSalesMetricsRepository = reportPurchaseSalesMetricsRepository;
        _reportPurchaseSalesMetricsResponseFactory = reportPurchaseSalesMetricsResponseFactory;
    }

    public async Task<Result<PagingDto<ReportPurchaseSalesMetricsPeriodDto>>> Handle(ListPurchaseSalesMetricsQuery request, CancellationToken cancellationToken)
    {
        var query = _reportPurchaseSalesMetricsRepository.AsQuerable();

        query = query.Where(x => x.Period == request.Period);

        query = query.OrderByDescending(x => x.Date);

        var result = await query.ToPaged(request.Skip, request.Length);

        var response = await _reportPurchaseSalesMetricsResponseFactory.PreparePagingDto(result);

        return response;
    }
}