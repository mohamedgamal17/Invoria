using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Procurement.Application.ReportPurchaseOrdersCompletedMetrics.Factories;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Domain.PurchaseOrders;
using Invoria.Procurement.Domain.Repositories;

namespace Invoria.Procurement.Application.ReportPurchaseOrdersCompletedMetrics.Queries.GetPurchaseOrdersCompletedMetrics;

using ReportPurchaseOrdersCompletedMetricsEntity = Invoria.Procurement.Domain.PurchaseOrders.ReportPurchaseOrdersCompletedMetrics;

public class GetPurchaseOrdersCompletedMetricsQueryHandler : IApplicatonRequestHandler<GetPurchaseOrdersCompletedMetricsQuery, ReportPurchaseOrdersCompletedMetricsDto>
{
    private readonly IProcurementRepository<ReportPurchaseOrdersCompletedMetricsEntity> _reportPurchaseOrdersCompletedMetricsRepository;
    private readonly IReportPurchaseOrdersCompletedMetricsResponseFactory _reportPurchaseOrdersCompletedMetricsResponseFactory;

    public GetPurchaseOrdersCompletedMetricsQueryHandler(
        IProcurementRepository<ReportPurchaseOrdersCompletedMetricsEntity> reportPurchaseOrdersCompletedMetricsRepository,
        IReportPurchaseOrdersCompletedMetricsResponseFactory reportPurchaseOrdersCompletedMetricsResponseFactory)
    {
        _reportPurchaseOrdersCompletedMetricsRepository = reportPurchaseOrdersCompletedMetricsRepository;
        _reportPurchaseOrdersCompletedMetricsResponseFactory = reportPurchaseOrdersCompletedMetricsResponseFactory;
    }

    public async Task<Result<ReportPurchaseOrdersCompletedMetricsDto>> Handle(GetPurchaseOrdersCompletedMetricsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.Now;

        var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
        var yearStart = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, now.Offset);

        var daily = await _reportPurchaseOrdersCompletedMetricsRepository.SingleOrDefault(
            x => x.Period == ReportPeriod.Daily && x.Date == todayStart, cancellationToken);
        var monthly = await _reportPurchaseOrdersCompletedMetricsRepository.SingleOrDefault(
            x => x.Period == ReportPeriod.Monthly && x.Date == monthStart, cancellationToken);
        var yearly = await _reportPurchaseOrdersCompletedMetricsRepository.SingleOrDefault(
            x => x.Period == ReportPeriod.Yearly && x.Date == yearStart, cancellationToken);
        var allTime = await _reportPurchaseOrdersCompletedMetricsRepository.SingleOrDefault(
            x => x.Period == ReportPeriod.AllTheTime, cancellationToken);

        daily ??= new ReportPurchaseOrdersCompletedMetricsEntity(todayStart, 0, ReportPeriod.Daily);
        monthly ??= new ReportPurchaseOrdersCompletedMetricsEntity(monthStart, 0, ReportPeriod.Monthly);
        yearly ??= new ReportPurchaseOrdersCompletedMetricsEntity(yearStart, 0, ReportPeriod.Yearly);
        allTime ??= new ReportPurchaseOrdersCompletedMetricsEntity(DateTimeOffset.MinValue, 0, ReportPeriod.AllTheTime);

        var dto = await _reportPurchaseOrdersCompletedMetricsResponseFactory.PrepareMetricsDto(daily, monthly, yearly, allTime);

        return dto;
    }
}