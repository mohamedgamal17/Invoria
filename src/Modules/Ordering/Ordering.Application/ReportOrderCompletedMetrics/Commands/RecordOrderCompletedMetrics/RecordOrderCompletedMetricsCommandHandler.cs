using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.ReportOrderCompletedMetrics.Commands.RecordOrderCompletedMetrics;

using ReportOrderCompletedMetricsEntity = Invoria.Ordering.Domain.Orders.ReportOrderCompletedMetrics;

public sealed class RecordOrderCompletedMetricsCommandHandler
    : IApplicatonRequestHandler<RecordOrderCompletedMetricsCommand, Empty>
{
    private readonly IOrderingRepository<ReportOrderCompletedMetricsEntity> _reportOrderCompletedMetricsRepository;

    public RecordOrderCompletedMetricsCommandHandler(
        IOrderingRepository<ReportOrderCompletedMetricsEntity> reportOrderCompletedMetricsRepository)
    {
        _reportOrderCompletedMetricsRepository = reportOrderCompletedMetricsRepository;
    }

    public async Task<Result<Empty>> Handle(RecordOrderCompletedMetricsCommand request, CancellationToken cancellationToken)
    {
        var occurredOn = request.OccurredOn;

        await UpsertReportAsync(
            ReportPeriod.Daily,
            new DateTimeOffset(occurredOn.Year, occurredOn.Month, occurredOn.Day, 0, 0, 0, occurredOn.Offset),
            cancellationToken);

        await UpsertReportAsync(
            ReportPeriod.Monthly,
            new DateTimeOffset(occurredOn.Year, occurredOn.Month, 1, 0, 0, 0, occurredOn.Offset),
            cancellationToken);

        await UpsertReportAsync(
            ReportPeriod.Yearly,
            new DateTimeOffset(occurredOn.Year, 1, 1, 0, 0, 0, occurredOn.Offset),
            cancellationToken);

        await UpsertReportAsync(
            ReportPeriod.AllTheTime,
            DateTimeOffset.MinValue,
            cancellationToken);

        return Result.Success(Empty.Value);
    }

    private async Task UpsertReportAsync(
        ReportPeriod period,
        DateTimeOffset date,
        CancellationToken cancellationToken)
    {
        var existing = await _reportOrderCompletedMetricsRepository
            .SingleOrDefault(x => x.Period == period && x.Date == date, cancellationToken);

        if (existing is null)
        {
            var newReport = new ReportOrderCompletedMetricsEntity(date, 1, period);

            await _reportOrderCompletedMetricsRepository.Add(newReport, cancellationToken);

            return;
        }

        existing.UpdateCount(existing.TotalCount + 1);

        await _reportOrderCompletedMetricsRepository.Update(existing, cancellationToken);
    }
}