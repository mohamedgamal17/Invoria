using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.OrderAllocationConsumptions;
using Invoria.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Ordering.Application.ReportOrderSalesProfitMetrics.Commands.RecordOrderSalesProfitMetrics;

using ReportOrderSalesProfitMetricsEntity = Invoria.Ordering.Domain.Orders.ReportOrderSalesProfitMetrics;

public sealed class RecordOrderSalesProfitMetricsCommandHandler
    : IApplicatonRequestHandler<RecordOrderSalesProfitMetricsCommand, Empty>
{
    private readonly IOrderingRepository<Order> _orderRepository;
    private readonly IOrderingRepository<OrderAllocationConsumption> _consumptionRepository;
    private readonly IOrderingRepository<ReportOrderSalesProfitMetricsEntity> _reportOrderSalesProfitMetricsRepository;

    public RecordOrderSalesProfitMetricsCommandHandler(
        IOrderingRepository<Order> orderRepository,
        IOrderingRepository<OrderAllocationConsumption> consumptionRepository,
        IOrderingRepository<ReportOrderSalesProfitMetricsEntity> reportOrderSalesProfitMetricsRepository)
    {
        _orderRepository = orderRepository;
        _consumptionRepository = consumptionRepository;
        _reportOrderSalesProfitMetricsRepository = reportOrderSalesProfitMetricsRepository;
    }

    public async Task<Result<Empty>> Handle(
        RecordOrderSalesProfitMetricsCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository
            .AsQuerable()
            .Include(o => o.Items)
            .Include(o => o.ReturnItems)
            .SingleOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure<Empty>(new InvalidOperationException(
                $"Order {request.OrderId} was not found."));
        }

        var consumption = await _consumptionRepository.SingleOrDefault(
            c => c.AllocationId == request.AllocationId,
            cancellationToken);

        if (consumption is null)
        {
            return Result.Failure<Empty>(new InvalidOperationException(
                $"Order allocation consumption for AllocationId={request.AllocationId} was not found."));
        }

        var returnedQuantities = order.ReturnItems
            .GroupBy(r => r.OrderItemId)
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Quantity));

        var totalRevenue = order.NetOfTotalOrderAmount;
        var totalCost = consumption.Lines.Sum(line => CalculateLineCost(line, returnedQuantities));
        var totalReturnAmount = order.TotalReturnAmount;

        var contribution = new ReportSalesProfitContribution(
            totalRevenue,
            totalCost,
            totalRevenue - totalCost,
            totalReturnAmount);

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

    private static decimal CalculateLineCost(
        OrderAllocationConsumptionLine line,
        IReadOnlyDictionary<string, int> returnedQuantities)
    {
        var returnedQuantity = returnedQuantities.TryGetValue(line.OrderItemId, out var quantity)
            ? quantity
            : 0;

        var batches = line.BatchAllocations.ToList();
        var remainingReturn = returnedQuantity;
        var totalCost = 0m;

        for (var i = batches.Count - 1; i >= 0; i--)
        {
            var batch = batches[i];

            var deductedQuantity = Math.Min(batch.Quantity, remainingReturn);
            remainingReturn -= deductedQuantity;

            var billableQuantity = batch.Quantity - deductedQuantity;

            totalCost += billableQuantity * batch.UnitPrice;
        }

        return totalCost;
    }

    private async Task UpsertReportAsync(
        ReportPeriod period,
        DateTimeOffset date,
        ReportSalesProfitContribution contribution,
        CancellationToken cancellationToken)
    {
        var existing = await _reportOrderSalesProfitMetricsRepository
            .SingleOrDefault(x => x.Period == period && x.Date == date, cancellationToken);

        if (existing is null)
        {
            var newReport = new ReportOrderSalesProfitMetricsEntity(
                date,
                contribution.TotalRevenue,
                contribution.TotalCost,
                contribution.TotalProfit,
                contribution.TotalReturnAmount,
                period);

            await _reportOrderSalesProfitMetricsRepository.Add(newReport, cancellationToken);

            return;
        }

        existing.UpdateAmounts(
            existing.TotalRevenue + contribution.TotalRevenue,
            existing.TotalCost + contribution.TotalCost,
            existing.TotalProfit + contribution.TotalProfit,
            existing.TotalReturnAmount + contribution.TotalReturnAmount);

        await _reportOrderSalesProfitMetricsRepository.Update(existing, cancellationToken);
    }

    private readonly record struct ReportSalesProfitContribution(
        decimal TotalRevenue,
        decimal TotalCost,
        decimal TotalProfit,
        decimal TotalReturnAmount);
}
