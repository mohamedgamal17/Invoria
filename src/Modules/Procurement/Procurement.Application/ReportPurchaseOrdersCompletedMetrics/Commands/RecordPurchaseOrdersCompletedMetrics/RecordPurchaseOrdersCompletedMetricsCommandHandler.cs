using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Procurement.Domain.PurchaseOrders;
using Invoria.Procurement.Domain.Repositories;

namespace Invoria.Procurement.Application.ReportPurchaseOrdersCompletedMetrics.Commands.RecordPurchaseOrdersCompletedMetrics;

using ReportPurchaseOrdersCompletedMetricsEntity = Invoria.Procurement.Domain.PurchaseOrders.ReportPurchaseOrdersCompletedMetrics;

public sealed class RecordPurchaseOrdersCompletedMetricsCommandHandler
    : IApplicatonRequestHandler<RecordPurchaseOrdersCompletedMetricsCommand, Empty>
{
    private readonly IProcurementRepository<ReportPurchaseOrdersCompletedMetricsEntity> _reportPurchaseOrdersCompletedMetricsRepository;

    public RecordPurchaseOrdersCompletedMetricsCommandHandler(
        IProcurementRepository<ReportPurchaseOrdersCompletedMetricsEntity> reportPurchaseOrdersCompletedMetricsRepository)
    {
        _reportPurchaseOrdersCompletedMetricsRepository = reportPurchaseOrdersCompletedMetricsRepository;
    }

    public async Task<Result<Empty>> Handle(RecordPurchaseOrdersCompletedMetricsCommand request, CancellationToken cancellationToken)
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
        var reportId = ReportPurchaseOrdersCompletedMetricsEntity.CreateId(period, date);

        var existing = await _reportPurchaseOrdersCompletedMetricsRepository
            .SingleOrDefault(x => x.Id == reportId, cancellationToken);

        if (existing is null)
        {
            var newReport = new ReportPurchaseOrdersCompletedMetricsEntity(reportId, date, 1, period);

            await _reportPurchaseOrdersCompletedMetricsRepository.Add(newReport, cancellationToken);

            return;
        }

        existing.UpdateCount(existing.TotalCount + 1);

        await _reportPurchaseOrdersCompletedMetricsRepository.Update(existing, cancellationToken);
    }
}