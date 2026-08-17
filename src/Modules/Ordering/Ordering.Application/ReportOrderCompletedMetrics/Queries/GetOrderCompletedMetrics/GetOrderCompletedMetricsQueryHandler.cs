using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Application.ReportOrderCompletedMetrics.Factories;
using Invoria.Ordering.Contracts.Orders.Dtos;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.ReportOrderCompletedMetrics.Queries.GetOrderCompletedMetrics;

using ReportOrderCompletedMetricsEntity = Invoria.Ordering.Domain.Orders.ReportOrderCompletedMetrics;

public class GetOrderCompletedMetricsQueryHandler : IApplicatonRequestHandler<GetOrderCompletedMetricsQuery, ReportOrderCompletedMetricsDto>
{
    private readonly IOrderingRepository<ReportOrderCompletedMetricsEntity> _reportOrderCompletedMetricsRepository;
    private readonly IReportOrderCompletedMetricsResponseFactory _reportOrderCompletedMetricsResponseFactory;

    public GetOrderCompletedMetricsQueryHandler(
        IOrderingRepository<ReportOrderCompletedMetricsEntity> reportOrderCompletedMetricsRepository,
        IReportOrderCompletedMetricsResponseFactory reportOrderCompletedMetricsResponseFactory)
    {
        _reportOrderCompletedMetricsRepository = reportOrderCompletedMetricsRepository;
        _reportOrderCompletedMetricsResponseFactory = reportOrderCompletedMetricsResponseFactory;
    }

    public async Task<Result<ReportOrderCompletedMetricsDto>> Handle(GetOrderCompletedMetricsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.Now;

        var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
        var yearStart = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, now.Offset);

        var daily = await _reportOrderCompletedMetricsRepository.SingleOrDefault(
            x => x.Period == ReportPeriod.Daily && x.Date == todayStart, cancellationToken);
        var monthly = await _reportOrderCompletedMetricsRepository.SingleOrDefault(
            x => x.Period == ReportPeriod.Monthly && x.Date == monthStart, cancellationToken);
        var yearly = await _reportOrderCompletedMetricsRepository.SingleOrDefault(
            x => x.Period == ReportPeriod.Yearly && x.Date == yearStart, cancellationToken);
        var allTime = await _reportOrderCompletedMetricsRepository.SingleOrDefault(
            x => x.Period == ReportPeriod.AllTheTime, cancellationToken);

        daily ??= new ReportOrderCompletedMetricsEntity(todayStart, 0, ReportPeriod.Daily);
        monthly ??= new ReportOrderCompletedMetricsEntity(monthStart, 0, ReportPeriod.Monthly);
        yearly ??= new ReportOrderCompletedMetricsEntity(yearStart, 0, ReportPeriod.Yearly);
        allTime ??= new ReportOrderCompletedMetricsEntity(DateTimeOffset.MinValue, 0, ReportPeriod.AllTheTime);

        var dto = await _reportOrderCompletedMetricsResponseFactory.PrepareMetricsDto(daily, monthly, yearly, allTime);

        return dto;
    }
}