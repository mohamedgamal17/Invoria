using Invoria.BackgroundJob.Core;
using Invoria.BackgroundJob.Core.Checkpoints;
using Invoria.BackgroundJob.Core.Context;
using Invoria.BackgroundJob.Core.Jobs;
using Invoria.BackgroundJobs.Abstractions.CheckPoints;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.Procurement.Domain.Parties;
using Invoria.Procurement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Procurement.Application.Parties.Jobs;

using ReportSupplierMetricsEntity = Invoria.Procurement.Domain.Parties.ReportSupplierMetrics;

public sealed class ReportSupplierMetricsJob : IJob
{
    public const string Name = "ReportSupplierMetricsJob";

    public static int BatchSize { get; set; } = 100;

    private readonly IJobCheckpointStore _jobCheckpointStore;
    private readonly IJobExecutionContextAccessor _jobExecutionContextAccessor;
    private readonly IProcurementRepository<Supplier> _supplierRepository;
    private readonly IProcurementRepository<ReportSupplierMetricsEntity> _reportSupplierMetricsRepository;

    public ReportSupplierMetricsJob(
        IJobCheckpointStore jobCheckpointStore,
        IJobExecutionContextAccessor jobExecutionContextAccessor,
        IProcurementRepository<Supplier> supplierRepository,
        IProcurementRepository<ReportSupplierMetricsEntity> reportSupplierMetricsRepository)
    {
        _jobCheckpointStore = jobCheckpointStore;
        _jobExecutionContextAccessor = jobExecutionContextAccessor;
        _supplierRepository = supplierRepository;
        _reportSupplierMetricsRepository = reportSupplierMetricsRepository;
    }

    public async Task Execute(CancellationToken cancellationToken = default)
    {
        var reportJobCheckPoint = await RestoreStateAsync(cancellationToken);

        var jobState = reportJobCheckPoint.GetState<ReportJobCheckPoint>()
            ?? new ReportJobCheckPoint();

        bool isBatchFull;

        do
        {
            var supplierQuery = _supplierRepository.AsQuerable();

            var orderedSuppliers = supplierQuery
                .OrderBy(x => x.Id)
                .Skip((int)jobState.LastIndex)
                .Take(BatchSize);

            var suppliers = await orderedSuppliers.ToListAsync(cancellationToken);

            if (suppliers.Count == 0)
            {
                break;
            }

            await UpdateDailyReportAsync(suppliers, cancellationToken);
            await UpdateMonthlyReportAsync(suppliers, cancellationToken);
            await UpdateYearlyReportAsync(suppliers, cancellationToken);
            await UpdateAllTimeReportAsync(suppliers, cancellationToken);

            var lastSupplier = suppliers.LastOrDefault();
            if (lastSupplier is not null)
            {
                jobState.LastId = lastSupplier.Id;
            }

            jobState.LastIndex += suppliers.Count;

            reportJobCheckPoint.SetState(jobState);

            isBatchFull = suppliers.Count == BatchSize;
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
        List<Supplier> suppliers,
        CancellationToken cancellationToken)
    {
        var dailyGroups = suppliers.GroupBy(s =>
            new DateTimeOffset(s.CreatedAt.Year, s.CreatedAt.Month, s.CreatedAt.Day, 0, 0, 0, s.CreatedAt.Offset));

        foreach (var group in dailyGroups)
        {
            await UpsertReportAsync(ReportPeriod.Daily, group.Key, group.LongCount(), cancellationToken);
        }
    }

    private async Task UpdateMonthlyReportAsync(
        List<Supplier> suppliers,
        CancellationToken cancellationToken)
    {
        var monthlyGroups = suppliers.GroupBy(s =>
            new DateTimeOffset(s.CreatedAt.Year, s.CreatedAt.Month, 1, 0, 0, 0, s.CreatedAt.Offset));

        foreach (var group in monthlyGroups)
        {
            await UpsertReportAsync(ReportPeriod.Monthly, group.Key, group.LongCount(), cancellationToken);
        }
    }

    private async Task UpdateYearlyReportAsync(
        List<Supplier> suppliers,
        CancellationToken cancellationToken)
    {
        var yearlyGroups = suppliers.GroupBy(s =>
            new DateTimeOffset(s.CreatedAt.Year, 1, 1, 0, 0, 0, s.CreatedAt.Offset));

        foreach (var group in yearlyGroups)
        {
            await UpsertReportAsync(ReportPeriod.Yearly, group.Key, group.LongCount(), cancellationToken);
        }
    }

    private async Task UpdateAllTimeReportAsync(
        List<Supplier> suppliers,
        CancellationToken cancellationToken)
    {
        long contribution = suppliers.Count;

        await UpsertReportAsync(ReportPeriod.AllTheTime, DateTimeOffset.MinValue, contribution, cancellationToken);
    }

    private async Task UpsertReportAsync(
        ReportPeriod period,
        DateTimeOffset date,
        long contribution,
        CancellationToken cancellationToken)
    {
        var existing = await _reportSupplierMetricsRepository
            .SingleOrDefault(x => x.Period == period && x.Date == date, cancellationToken);

        if (existing is null)
        {
            var newReport = new ReportSupplierMetricsEntity(date, contribution, period);

            await _reportSupplierMetricsRepository.Add(newReport, cancellationToken);

            return;
        }

        existing.UpdateCount(existing.TotalCount + contribution);

        await _reportSupplierMetricsRepository.Update(existing, cancellationToken);
    }
}