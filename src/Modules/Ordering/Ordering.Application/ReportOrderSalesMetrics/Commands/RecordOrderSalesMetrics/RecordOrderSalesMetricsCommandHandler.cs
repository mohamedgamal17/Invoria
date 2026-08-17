using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Ordering.Application.ReportOrderSalesMetrics.Commands.RecordOrderSalesMetrics;

using ReportOrderSalesMetricsEntity = Invoria.Ordering.Domain.Orders.ReportOrderSalesMetrics;

public sealed class RecordOrderSalesMetricsCommandHandler
    : IApplicatonRequestHandler<RecordOrderSalesMetricsCommand, Empty>
{
    private readonly IOrderingRepository<Order> _orderRepository;
    private readonly IOrderingRepository<ReportOrderSalesMetricsEntity> _reportOrderSalesMetricsRepository;

    public RecordOrderSalesMetricsCommandHandler(
        IOrderingRepository<Order> orderRepository,
        IOrderingRepository<ReportOrderSalesMetricsEntity> reportOrderSalesMetricsRepository)
    {
        _orderRepository = orderRepository;
        _reportOrderSalesMetricsRepository = reportOrderSalesMetricsRepository;
    }

    public async Task<Result<Empty>> Handle(RecordOrderSalesMetricsCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.AsQuerable()
            .Include(o => o.Items)
            .Include(o => o.ReturnItems)
            .SingleAsync(o => o.Id == request.OrderId, cancellationToken);

        var contribution = new ReportSalesContribution(
            order.TotalOrderAmount,
            order.NetOfTotalOrderAmount,
            order.TotalReturnAmount);

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
        ReportSalesContribution contribution,
        CancellationToken cancellationToken)
    {
        var existing = await _reportOrderSalesMetricsRepository
            .SingleOrDefault(x => x.Period == period && x.Date == date, cancellationToken);

        if (existing is null)
        {
            var newReport = new ReportOrderSalesMetricsEntity(
                date,
                contribution.TotalAmount,
                contribution.TotalNetAmount,
                contribution.TotalReturnAmount,
                period);

            await _reportOrderSalesMetricsRepository.Add(newReport, cancellationToken);

            return;
        }

        existing.UpdateAmounts(
            existing.TotalAmount + contribution.TotalAmount,
            existing.TotalNetAmount + contribution.TotalNetAmount,
            existing.TotalReturnAmount + contribution.TotalReturnAmount);

        await _reportOrderSalesMetricsRepository.Update(existing, cancellationToken);
    }

    private readonly record struct ReportSalesContribution(decimal TotalAmount, decimal TotalNetAmount, decimal TotalReturnAmount);
}