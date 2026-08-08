using Invoria.BackgroundJob.Core;
using Invoria.BackgroundJob.Core.Checkpoints;
using Invoria.BackgroundJob.Core.Context;
using Invoria.BackgroundJob.Core.Jobs;
using Invoria.BackgroundJobs.Abstractions.CheckPoints;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Ordering.Application.ReportOrderSalesMetrics.Jobs;

using ReportOrderSalesMetricsEntity = Invoria.Ordering.Domain.Orders.ReportOrderSalesMetrics;

public sealed class ReportOrderSalesMetricsJob : IJob
{
    public const string Name = "ReportOrderSalesMetricsJob";

    public static int BatchSize { get; set; } = 100;

    private readonly IJobCheckpointStore _jobCheckpointStore;
    private readonly IJobExecutionContextAccessor _jobExecutionContextAccessor;
    private readonly IOrderingRepository<OrderStateTransitionHistory> _stateTransitionHistoryRepository;
    private readonly IOrderingRepository<ReportOrderSalesMetricsEntity> _reportOrderSalesMetricsRepository;

    public ReportOrderSalesMetricsJob(
        IJobCheckpointStore jobCheckpointStore,
        IJobExecutionContextAccessor jobExecutionContextAccessor,
        IOrderingRepository<OrderStateTransitionHistory> stateTransitionHistoryRepository,
        IOrderingRepository<ReportOrderSalesMetricsEntity> reportOrderSalesMetricsRepository)
    {
        _jobCheckpointStore = jobCheckpointStore;
        _jobExecutionContextAccessor = jobExecutionContextAccessor;
        _stateTransitionHistoryRepository = stateTransitionHistoryRepository;
        _reportOrderSalesMetricsRepository = reportOrderSalesMetricsRepository;
    }

    public async Task Execute(CancellationToken cancellationToken = default)
    {
        var reportJobCheckPoint = await RestoreStateAsync(cancellationToken);

        var jobState = reportJobCheckPoint.GetState<ReportJobCheckPoint>()
            ?? new ReportJobCheckPoint();

        bool isBatchFull;

        do
        {
            var historyQuery = _stateTransitionHistoryRepository.AsQuerable();

            var orderedHistory = historyQuery
                .Where(x => x.ToStatus == OrderStatus.Completed)
                .Include(x => x.Order!)
                .ThenInclude(x => x.Items)
                .Include(x => x.Order!)
                .ThenInclude(x => x.ReturnItems)
                .OrderBy(x => x.Id)
                .Skip((int)jobState.LastIndex)
                .Take(BatchSize);

            var transitions = await orderedHistory.ToListAsync(cancellationToken);

            if (transitions.Count == 0)
            {
                break;
            }

            await UpdateDailyReportAsync(transitions, cancellationToken);
            await UpdateMonthlyReportAsync(transitions, cancellationToken);
            await UpdateYearlyReportAsync(transitions, cancellationToken);
            await UpdateAllTimeReportAsync(transitions, cancellationToken);

            var lastTransition = transitions.LastOrDefault();
            if (lastTransition is not null)
            {
                jobState.LastId = lastTransition.Id;
            }

            jobState.LastIndex += transitions.Count;

            reportJobCheckPoint.SetState(jobState);

            isBatchFull = transitions.Count == BatchSize;
        }
        while (isBatchFull);

        await _jobCheckpointStore.SaveAsync(reportJobCheckPoint, cancellationToken);
    }

    private async Task<JobCheckpoint> RestoreStateAsync(
        CancellationToken cancellationToken)
    {
        JobId jobId = _jobExecutionContextAccessor.JobId ?? new JobId(Name);

        var reportJobCheckPoint = await _jobCheckpointStore
            .RestoreAsync(jobId, cancellationToken);

        if (reportJobCheckPoint is null)
        {
            reportJobCheckPoint = new JobCheckpoint
            {
                JobId = jobId,
                Name = Name
            };
        }

        return reportJobCheckPoint;
    }

    private async Task UpdateDailyReportAsync(
        List<OrderStateTransitionHistory> transitions,
        CancellationToken cancellationToken)
    {
        var dailyGroups = transitions.GroupBy(t =>
            new DateTimeOffset(t.ChangedAt.Year, t.ChangedAt.Month, t.ChangedAt.Day, 0, 0, 0, t.ChangedAt.Offset));

        foreach (var group in dailyGroups)
        {
            var contribution = SumContribution(group);

            await UpsertReportAsync(ReportPeriod.Daily, group.Key, contribution, cancellationToken);
        }
    }

    private async Task UpdateMonthlyReportAsync(
        List<OrderStateTransitionHistory> transitions,
        CancellationToken cancellationToken)
    {
        var monthlyGroups = transitions.GroupBy(t =>
            new DateTimeOffset(t.ChangedAt.Year, t.ChangedAt.Month, 1, 0, 0, 0, t.ChangedAt.Offset));

        foreach (var group in monthlyGroups)
        {
            var contribution = SumContribution(group);

            await UpsertReportAsync(ReportPeriod.Monthly, group.Key, contribution, cancellationToken);
        }
    }

    private async Task UpdateYearlyReportAsync(
        List<OrderStateTransitionHistory> transitions,
        CancellationToken cancellationToken)
    {
        var yearlyGroups = transitions.GroupBy(t =>
            new DateTimeOffset(t.ChangedAt.Year, 1, 1, 0, 0, 0, t.ChangedAt.Offset));

        foreach (var group in yearlyGroups)
        {
            var contribution = SumContribution(group);

            await UpsertReportAsync(ReportPeriod.Yearly, group.Key, contribution, cancellationToken);
        }
    }

    private async Task UpdateAllTimeReportAsync(
        List<OrderStateTransitionHistory> transitions,
        CancellationToken cancellationToken)
    {
        var contribution = SumContribution(transitions);

        await UpsertReportAsync(ReportPeriod.AllTheTime, DateTimeOffset.MinValue, contribution, cancellationToken);
    }

    private static ReportSalesContribution SumContribution(IEnumerable<OrderStateTransitionHistory> transitions)
    {
        decimal totalAmount = 0m;
        decimal totalNetAmount = 0m;
        decimal totalReturnAmount = 0m;

        foreach (var transition in transitions)
        {
            var order = transition.Order;

            if (order is null)
            {
                continue;
            }

            totalAmount += order.TotalOrderAmount;
            totalNetAmount += order.NetOfTotalOrderAmount;
            totalReturnAmount += order.TotalReturnAmount;
        }

        return new ReportSalesContribution(totalAmount, totalNetAmount, totalReturnAmount);
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