using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Application.ReportOrderSalesProfitMetrics.Factories;
using Invoria.Ordering.Contracts.Orders.Dtos;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.ReportOrderSalesProfitMetrics.Queries.GetOrderSalesProfitMetrics;

using ReportOrderSalesProfitMetricsEntity = Invoria.Ordering.Domain.Orders.ReportOrderSalesProfitMetrics;

public class GetOrderSalesProfitMetricsQueryHandler : IApplicatonRequestHandler<GetOrderSalesProfitMetricsQuery, ReportOrderSalesProfitMetricsDto>
{
    private readonly IOrderingRepository<ReportOrderSalesProfitMetricsEntity> _reportOrderSalesProfitMetricsRepository;
    private readonly IReportOrderSalesProfitMetricsResponseFactory _reportOrderSalesProfitMetricsResponseFactory;

    public GetOrderSalesProfitMetricsQueryHandler(
        IOrderingRepository<ReportOrderSalesProfitMetricsEntity> reportOrderSalesProfitMetricsRepository,
        IReportOrderSalesProfitMetricsResponseFactory reportOrderSalesProfitMetricsResponseFactory)
    {
        _reportOrderSalesProfitMetricsRepository = reportOrderSalesProfitMetricsRepository;
        _reportOrderSalesProfitMetricsResponseFactory = reportOrderSalesProfitMetricsResponseFactory;
    }

    public async Task<Result<ReportOrderSalesProfitMetricsDto>> Handle(GetOrderSalesProfitMetricsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.Now;

        var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
        var yearStart = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, now.Offset);

        var daily = await _reportOrderSalesProfitMetricsRepository.SingleOrDefault(
            x => x.Period == ReportPeriod.Daily && x.Date == todayStart, cancellationToken);
        var monthly = await _reportOrderSalesProfitMetricsRepository.SingleOrDefault(
            x => x.Period == ReportPeriod.Monthly && x.Date == monthStart, cancellationToken);
        var yearly = await _reportOrderSalesProfitMetricsRepository.SingleOrDefault(
            x => x.Period == ReportPeriod.Yearly && x.Date == yearStart, cancellationToken);
        var allTime = await _reportOrderSalesProfitMetricsRepository.SingleOrDefault(
            x => x.Period == ReportPeriod.AllTheTime, cancellationToken);

        daily ??= new ReportOrderSalesProfitMetricsEntity(todayStart, 0m, 0m, 0m, 0m, ReportPeriod.Daily);
        monthly ??= new ReportOrderSalesProfitMetricsEntity(monthStart, 0m, 0m, 0m, 0m, ReportPeriod.Monthly);
        yearly ??= new ReportOrderSalesProfitMetricsEntity(yearStart, 0m, 0m, 0m, 0m, ReportPeriod.Yearly);
        allTime ??= new ReportOrderSalesProfitMetricsEntity(DateTimeOffset.MinValue, 0m, 0m, 0m, 0m, ReportPeriod.AllTheTime);

        var dto = await _reportOrderSalesProfitMetricsResponseFactory.PrepareMetricsDto(daily, monthly, yearly, allTime);

        return dto;
    }
}