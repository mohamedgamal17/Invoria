using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Procurement.Domain.PurchaseOrders;
using Invoria.Procurement.Domain.Repositories;

namespace Invoria.Procurement.Application.ReportPurchaseSalesMetrics.Commands.RecordPurchaseSalesMetrics;

using ReportPurchaseSalesMetricsEntity = Invoria.Procurement.Domain.PurchaseOrders.ReportPurchaseSalesMetrics;

public sealed class RecordPurchaseSalesMetricsCommandHandler
    : IApplicatonRequestHandler<RecordPurchaseSalesMetricsCommand, Empty>
{
    private readonly IProcurementRepository<PurchaseOrder> _purchaseOrderRepository;
    private readonly IProcurementRepository<ReportPurchaseSalesMetricsEntity> _reportPurchaseSalesMetricsRepository;

    public RecordPurchaseSalesMetricsCommandHandler(
        IProcurementRepository<PurchaseOrder> purchaseOrderRepository,
        IProcurementRepository<ReportPurchaseSalesMetricsEntity> reportPurchaseSalesMetricsRepository)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _reportPurchaseSalesMetricsRepository = reportPurchaseSalesMetricsRepository;
    }

    public async Task<Result<Empty>> Handle(RecordPurchaseSalesMetricsCommand request, CancellationToken cancellationToken)
    {
        var purchaseOrder = await _purchaseOrderRepository
            .SingleOrDefault(x => x.Id == request.PurchaseOrderId, cancellationToken);

        if (purchaseOrder is null)
        {
            return Result.Failure<Empty>(new InvalidOperationException(
                $"Purchase order {request.PurchaseOrderId} was not found."));
        }

        var contribution = new ReportPurchaseSalesContribution(
            purchaseOrder.TotalAmount,
            purchaseOrder.SubTotal,
            purchaseOrder.TaxAmount,
            purchaseOrder.DiscountAmount);

        var occurredOn = request.OccurredOn;

        await UpsertReportAsync(
            ReportPeriod.Daily,
            new DateTimeOffset(occurredOn.Year, occurredOn.Month, occurredOn.Day, 0, 0, 0, occurredOn.Offset),
            contribution,
            cancellationToken);

        await UpsertReportAsync(
            ReportPeriod.Monthly,
            new DateTimeOffset(occurredOn.Year, occurredOn.Month, 1, 0, 0, 0, occurredOn.Offset),
            contribution,
            cancellationToken);

        await UpsertReportAsync(
            ReportPeriod.Yearly,
            new DateTimeOffset(occurredOn.Year, 1, 1, 0, 0, 0, occurredOn.Offset),
            contribution,
            cancellationToken);

        await UpsertReportAsync(
            ReportPeriod.AllTheTime,
            DateTimeOffset.MinValue,
            contribution,
            cancellationToken);

        return Result.Success(Empty.Value);
    }

    private async Task UpsertReportAsync(
        ReportPeriod period,
        DateTimeOffset date,
        ReportPurchaseSalesContribution contribution,
        CancellationToken cancellationToken)
    {
        var existing = await _reportPurchaseSalesMetricsRepository
            .SingleOrDefault(x => x.Period == period && x.Date == date, cancellationToken);

        if (existing is null)
        {
            var newReport = new ReportPurchaseSalesMetricsEntity(
                date,
                contribution.TotalAmount,
                contribution.SubTotal,
                contribution.TaxAmount,
                contribution.DiscountAmount,
                period);

            await _reportPurchaseSalesMetricsRepository.Add(newReport, cancellationToken);

            return;
        }

        existing.UpdateAmounts(
            existing.TotalAmount + contribution.TotalAmount,
            existing.SubTotal + contribution.SubTotal,
            existing.TaxAmount + contribution.TaxAmount,
            existing.DiscountAmount + contribution.DiscountAmount);

        await _reportPurchaseSalesMetricsRepository.Update(existing, cancellationToken);
    }

    private readonly record struct ReportPurchaseSalesContribution(
        decimal TotalAmount,
        decimal SubTotal,
        decimal TaxAmount,
        decimal DiscountAmount);
}