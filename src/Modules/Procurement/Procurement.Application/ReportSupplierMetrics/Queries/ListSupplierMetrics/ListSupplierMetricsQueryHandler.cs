using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Procurement.Application.ReportSupplierMetrics.Factories;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Domain.Parties;
using Invoria.Procurement.Domain.Repositories;

namespace Invoria.Procurement.Application.ReportSupplierMetrics.Queries.ListSupplierMetrics;

using ReportSupplierMetricsEntity = Invoria.Procurement.Domain.Parties.ReportSupplierMetrics;

public class ListSupplierMetricsQueryHandler : IApplicatonRequestHandler<ListSupplierMetricsQuery, PagingDto<ReportSupplierMetricsPeriodDto>>
{
    private readonly IProcurementRepository<ReportSupplierMetricsEntity> _reportSupplierMetricsRepository;
    private readonly IReportSupplierMetricsResponseFactory _reportSupplierMetricsResponseFactory;

    public ListSupplierMetricsQueryHandler(
        IProcurementRepository<ReportSupplierMetricsEntity> reportSupplierMetricsRepository,
        IReportSupplierMetricsResponseFactory reportSupplierMetricsResponseFactory)
    {
        _reportSupplierMetricsRepository = reportSupplierMetricsRepository;
        _reportSupplierMetricsResponseFactory = reportSupplierMetricsResponseFactory;
    }

    public async Task<Result<PagingDto<ReportSupplierMetricsPeriodDto>>> Handle(ListSupplierMetricsQuery request, CancellationToken cancellationToken)
    {
        var query = _reportSupplierMetricsRepository.AsQuerable();

        query = query.Where(x => x.Period == request.Period);

        query = query.OrderByDescending(x => x.Date);

        var result = await query.ToPaged(request.Skip, request.Length);

        var response = await _reportSupplierMetricsResponseFactory.PreparePagingDto(result);

        return response;
    }
}