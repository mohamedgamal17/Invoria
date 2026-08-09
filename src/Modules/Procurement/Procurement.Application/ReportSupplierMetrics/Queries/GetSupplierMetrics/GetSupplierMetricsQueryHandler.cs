using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Procurement.Application.ReportSupplierMetrics.Factories;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Domain.Parties;
using Invoria.Procurement.Domain.Repositories;

namespace Invoria.Procurement.Application.ReportSupplierMetrics.Queries.GetSupplierMetrics;

using ReportSupplierMetricsEntity = Invoria.Procurement.Domain.Parties.ReportSupplierMetrics;

public class GetSupplierMetricsQueryHandler : IApplicatonRequestHandler<GetSupplierMetricsQuery, ReportSupplierMetricsDto>
{
    private readonly IProcurementRepository<ReportSupplierMetricsEntity> _reportSupplierMetricsRepository;
    private readonly IReportSupplierMetricsResponseFactory _reportSupplierMetricsResponseFactory;

    public GetSupplierMetricsQueryHandler(
        IProcurementRepository<ReportSupplierMetricsEntity> reportSupplierMetricsRepository,
        IReportSupplierMetricsResponseFactory reportSupplierMetricsResponseFactory)
    {
        _reportSupplierMetricsRepository = reportSupplierMetricsRepository;
        _reportSupplierMetricsResponseFactory = reportSupplierMetricsResponseFactory;
    }

    public async Task<Result<ReportSupplierMetricsDto>> Handle(GetSupplierMetricsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.Now;

        var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
        var yearStart = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, now.Offset);

        var daily = await _reportSupplierMetricsRepository.SingleOrDefault(
            x => x.Period == ReportPeriod.Daily && x.Date == todayStart, cancellationToken);
        var monthly = await _reportSupplierMetricsRepository.SingleOrDefault(
            x => x.Period == ReportPeriod.Monthly && x.Date == monthStart, cancellationToken);
        var yearly = await _reportSupplierMetricsRepository.SingleOrDefault(
            x => x.Period == ReportPeriod.Yearly && x.Date == yearStart, cancellationToken);
        var allTime = await _reportSupplierMetricsRepository.SingleOrDefault(
            x => x.Period == ReportPeriod.AllTheTime, cancellationToken);

        daily ??= new ReportSupplierMetricsEntity(todayStart, 0, ReportPeriod.Daily);
        monthly ??= new ReportSupplierMetricsEntity(monthStart, 0, ReportPeriod.Monthly);
        yearly ??= new ReportSupplierMetricsEntity(yearStart, 0, ReportPeriod.Yearly);
        allTime ??= new ReportSupplierMetricsEntity(DateTimeOffset.MinValue, 0, ReportPeriod.AllTheTime);

        var dto = await _reportSupplierMetricsResponseFactory.PrepareMetricsDto(daily, monthly, yearly, allTime);

        return dto;
    }
}