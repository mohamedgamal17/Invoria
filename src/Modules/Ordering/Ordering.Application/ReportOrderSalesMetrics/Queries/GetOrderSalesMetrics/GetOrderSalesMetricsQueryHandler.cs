using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Application.ReportOrderSalesMetrics.Factories;
using Invoria.Ordering.Contracts.Orders.Dtos;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.ReportOrderSalesMetrics.Queries.GetOrderSalesMetrics;

using ReportOrderSalesMetricsEntity = Invoria.Ordering.Domain.Orders.ReportOrderSalesMetrics;

public class GetOrderSalesMetricsQueryHandler : IApplicatonRequestHandler<GetOrderSalesMetricsQuery, ReportOrderSalesMetricsDto>
{
    private readonly IOrderingRepository<ReportOrderSalesMetricsEntity> _reportOrderSalesMetricsRepository;
    private readonly IReportOrderSalesMetricsResponseFactory _reportOrderSalesMetricsResponseFactory;

    public GetOrderSalesMetricsQueryHandler(
        IOrderingRepository<ReportOrderSalesMetricsEntity> reportOrderSalesMetricsRepository,
        IReportOrderSalesMetricsResponseFactory reportOrderSalesMetricsResponseFactory)
    {
        _reportOrderSalesMetricsRepository = reportOrderSalesMetricsRepository;
        _reportOrderSalesMetricsResponseFactory = reportOrderSalesMetricsResponseFactory;
    }

    public async Task<Result<ReportOrderSalesMetricsDto>> Handle(GetOrderSalesMetricsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.Now;

        var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
        var yearStart = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, now.Offset);

        var daily = await _reportOrderSalesMetricsRepository.SingleOrDefault(
            x => x.Period == ReportPeriod.Daily && x.Date == todayStart, cancellationToken);
        var monthly = await _reportOrderSalesMetricsRepository.SingleOrDefault(
            x => x.Period == ReportPeriod.Monthly && x.Date == monthStart, cancellationToken);
        var yearly = await _reportOrderSalesMetricsRepository.SingleOrDefault(
            x => x.Period == ReportPeriod.Yearly && x.Date == yearStart, cancellationToken);
        var allTime = await _reportOrderSalesMetricsRepository.SingleOrDefault(
            x => x.Period == ReportPeriod.AllTheTime, cancellationToken);

        daily ??= new ReportOrderSalesMetricsEntity(todayStart, 0m, 0m, 0m, ReportPeriod.Daily);
        monthly ??= new ReportOrderSalesMetricsEntity(monthStart, 0m, 0m, 0m, ReportPeriod.Monthly);
        yearly ??= new ReportOrderSalesMetricsEntity(yearStart, 0m, 0m, 0m, ReportPeriod.Yearly);
        allTime ??= new ReportOrderSalesMetricsEntity(DateTimeOffset.MinValue, 0m, 0m, 0m, ReportPeriod.AllTheTime);

        var dto = await _reportOrderSalesMetricsResponseFactory.PrepareMetricsDto(daily, monthly, yearly, allTime);

        return dto;
    }
}