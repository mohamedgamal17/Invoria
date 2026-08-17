using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Procurement.Application.ReportPurchaseSalesMetrics.Factories;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Domain.PurchaseOrders;
using Invoria.Procurement.Domain.Repositories;

namespace Invoria.Procurement.Application.ReportPurchaseSalesMetrics.Queries.GetPurchaseSalesMetrics;

using ReportPurchaseSalesMetricsEntity = Invoria.Procurement.Domain.PurchaseOrders.ReportPurchaseSalesMetrics;

public class GetPurchaseSalesMetricsQueryHandler : IApplicatonRequestHandler<GetPurchaseSalesMetricsQuery, ReportPurchaseSalesMetricsDto>
{
    private readonly IProcurementRepository<ReportPurchaseSalesMetricsEntity> _reportPurchaseSalesMetricsRepository;
    private readonly IReportPurchaseSalesMetricsResponseFactory _reportPurchaseSalesMetricsResponseFactory;

    public GetPurchaseSalesMetricsQueryHandler(
        IProcurementRepository<ReportPurchaseSalesMetricsEntity> reportPurchaseSalesMetricsRepository,
        IReportPurchaseSalesMetricsResponseFactory reportPurchaseSalesMetricsResponseFactory)
    {
        _reportPurchaseSalesMetricsRepository = reportPurchaseSalesMetricsRepository;
        _reportPurchaseSalesMetricsResponseFactory = reportPurchaseSalesMetricsResponseFactory;
    }

    public async Task<Result<ReportPurchaseSalesMetricsDto>> Handle(GetPurchaseSalesMetricsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.Now;

        var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
        var yearStart = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, now.Offset);

        var daily = await _reportPurchaseSalesMetricsRepository.SingleOrDefault(
            x => x.Period == ReportPeriod.Daily && x.Date == todayStart, cancellationToken);
        var monthly = await _reportPurchaseSalesMetricsRepository.SingleOrDefault(
            x => x.Period == ReportPeriod.Monthly && x.Date == monthStart, cancellationToken);
        var yearly = await _reportPurchaseSalesMetricsRepository.SingleOrDefault(
            x => x.Period == ReportPeriod.Yearly && x.Date == yearStart, cancellationToken);
        var allTime = await _reportPurchaseSalesMetricsRepository.SingleOrDefault(
            x => x.Period == ReportPeriod.AllTheTime, cancellationToken);

        daily ??= new ReportPurchaseSalesMetricsEntity(todayStart, 0m, 0m, 0m, 0m, ReportPeriod.Daily);
        monthly ??= new ReportPurchaseSalesMetricsEntity(monthStart, 0m, 0m, 0m, 0m, ReportPeriod.Monthly);
        yearly ??= new ReportPurchaseSalesMetricsEntity(yearStart, 0m, 0m, 0m, 0m, ReportPeriod.Yearly);
        allTime ??= new ReportPurchaseSalesMetricsEntity(DateTimeOffset.MinValue, 0m, 0m, 0m, 0m, ReportPeriod.AllTheTime);

        var dto = await _reportPurchaseSalesMetricsResponseFactory.PrepareMetricsDto(daily, monthly, yearly, allTime);

        return dto;
    }
}