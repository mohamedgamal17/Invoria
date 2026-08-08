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

namespace Invoria.Ordering.Application.Orders.Jobs;

using ReportOrderCompletedMetricsEntity = Invoria.Ordering.Domain.Orders.ReportOrderCompletedMetrics;

public sealed class ReportOrderCompletedMetricsJob : IJob
{
    public const string Name = "ReportOrderCompletedMetricsJob";

    public static int BatchSize { get; set; } = 100;

    private readonly IJobCheckpointStore _jobCheckpointStore;
    private readonly IJobExecutionContextAccessor _jobExecutionContextAccessor;
    private readonly IOrderingRepository<OrderStateTransitionHistory> _stateTransitionHistoryRepository;
    private readonly IOrderingRepository<ReportOrderCompletedMetricsEntity> _reportOrderCompletedMetricsRepository;

    public ReportOrderCompletedMetricsJob(
        IJobCheckpointStore jobCheckpointStore,
        IJobExecutionContextAccessor jobExecutionContextAccessor,
        IOrderingRepository<OrderStateTransitionHistory> stateTransitionHistoryRepository,
        IOrderingRepository<ReportOrderCompletedMetricsEntity> reportOrderCompletedMetricsRepository)
    {
        _jobCheckpointStore = jobCheckpointStore;
        _jobExecutionContextAccessor = jobExecutionContextAccessor;
        _stateTransitionHistoryRepository = stateTransitionHistoryRepository;
        _reportOrderCompletedMetricsRepository = reportOrderCompletedMetricsRepository;
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
            await UpsertReportAsync(ReportPeriod.Daily, group.Key, group.LongCount(), cancellationToken);
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
            await UpsertReportAsync(ReportPeriod.Monthly, group.Key, group.LongCount(), cancellationToken);
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
            await UpsertReportAsync(ReportPeriod.Yearly, group.Key, group.LongCount(), cancellationToken);
        }
    }

    private async Task UpdateAllTimeReportAsync(
        List<OrderStateTransitionHistory> transitions,
        CancellationToken cancellationToken)
    {
        long contribution = transitions.Count;

        await UpsertReportAsync(ReportPeriod.AllTheTime, DateTimeOffset.MinValue, contribution, cancellationToken);
    }

    private async Task UpsertReportAsync(
        ReportPeriod period,
        DateTimeOffset date,
        long contribution,
        CancellationToken cancellationToken)
    {
        var existing = await _reportOrderCompletedMetricsRepository
            .SingleOrDefault(x => x.Period == period && x.Date == date, cancellationToken);

        if (existing is null)
        {
            var newReport = new ReportOrderCompletedMetricsEntity(date, contribution, period);

            await _reportOrderCompletedMetricsRepository.Add(newReport, cancellationToken);

            return;
        }

        existing.UpdateCount(existing.TotalCount + contribution);

        await _reportOrderCompletedMetricsRepository.Update(existing, cancellationToken);
    }
}
