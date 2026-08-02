using Invoria.BackgroundJob.Core;
using Invoria.BackgroundJob.Core.Checkpoints;
using Invoria.BackgroundJob.Core.Context;
using Invoria.BackgroundJob.Core.Jobs;
using Invoria.BackgroundJobs.Abstractions.CheckPoints;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.Catalog.Domain;
using Invoria.Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Catalog.Application.Products.Jobs;

public sealed class ReportProductMetricsJob : IJob
{
    public const string Name = "ReportProductMetricsJob";

    public static int BatchSize { get; set; } = 100;

    private readonly IJobCheckpointStore _jobCheckpointStore;
    private readonly IJobExecutionContextAccessor _jobExecutionContextAccessor;
    private readonly ICatalogRepository<Product> _productRepository;
    private readonly ICatalogRepository<ReportProductMetrics> _reportProductMetricsRepository;

    public ReportProductMetricsJob(
        IJobCheckpointStore jobCheckpointStore,
        IJobExecutionContextAccessor jobExecutionContextAccessor,
        ICatalogRepository<Product> productRepository,
        ICatalogRepository<ReportProductMetrics> reportProductMetricsRepository)
    {
        _jobCheckpointStore = jobCheckpointStore;
        _jobExecutionContextAccessor = jobExecutionContextAccessor;
        _productRepository = productRepository;
        _reportProductMetricsRepository = reportProductMetricsRepository;
    }

    public async Task Execute(CancellationToken cancellationToken = default)
    {
        var reportJobCheckPoint = await RestoreStateAsync(cancellationToken);

        var jobState = reportJobCheckPoint.GetState<ReportJobCheckPoint>()
            ?? new ReportJobCheckPoint();

        bool isBatchFull;

        do
        {
            var productQuery = _productRepository.AsQuerable();

            var orderedProducts = productQuery
                .OrderBy(x => x.Id)
                .Skip((int)jobState.LastIndex)
                .Take(BatchSize);

            var products = await orderedProducts.ToListAsync(cancellationToken);

            if (products.Count == 0)
            {
                break;
            }

            await UpdateDailyReportAsync(products, cancellationToken);
            await UpdateMonthlyReportAsync(products, cancellationToken);
            await UpdateYearlyReportAsync(products, cancellationToken);
            await UpdateAllTimeReportAsync(products, cancellationToken);

            var lastProduct = products.LastOrDefault();
            if (lastProduct is not null)
            {
                jobState.LastId = lastProduct.Id;
            }

            jobState.LastIndex += products.Count;

            reportJobCheckPoint.SetState(jobState);

            isBatchFull = products.Count == BatchSize;
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
        List<Product> products,
        CancellationToken cancellationToken)
    {
        var dailyGroups = products.GroupBy(p =>
            new DateTimeOffset(p.CreatedAt.Year, p.CreatedAt.Month, p.CreatedAt.Day, 0, 0, 0, p.CreatedAt.Offset));

        foreach (var group in dailyGroups)
        {
            await UpsertReportAsync(ReportPeriod.Daily, group.Key, group.LongCount(), cancellationToken);
        }
    }

    private async Task UpdateMonthlyReportAsync(
        List<Product> products,
        CancellationToken cancellationToken)
    {
        var monthlyGroups = products.GroupBy(p =>
            new DateTimeOffset(p.CreatedAt.Year, p.CreatedAt.Month, 1, 0, 0, 0, p.CreatedAt.Offset));

        foreach (var group in monthlyGroups)
        {
            await UpsertReportAsync(ReportPeriod.Monthly, group.Key, group.LongCount(), cancellationToken);
        }
    }

    private async Task UpdateYearlyReportAsync(
        List<Product> products,
        CancellationToken cancellationToken)
    {
        var yearlyGroups = products.GroupBy(p =>
            new DateTimeOffset(p.CreatedAt.Year, 1, 1, 0, 0, 0, p.CreatedAt.Offset));

        foreach (var group in yearlyGroups)
        {
            await UpsertReportAsync(ReportPeriod.Yearly, group.Key, group.LongCount(), cancellationToken);
        }
    }

    private async Task UpdateAllTimeReportAsync(
        List<Product> products,
        CancellationToken cancellationToken)
    {
        long contribution = products.Count;

        await UpsertReportAsync(ReportPeriod.AllTheTime, DateTimeOffset.MinValue, contribution, cancellationToken);
    }

    private async Task UpsertReportAsync(
        ReportPeriod period,
        DateTimeOffset date,
        long contribution,
        CancellationToken cancellationToken)
    {
        var existing = await _reportProductMetricsRepository
            .SingleOrDefault(x => x.Period == period && x.Date == date, cancellationToken);

        if (existing is null)
        {
            var newReport = new ReportProductMetrics(date, contribution, period);

            await _reportProductMetricsRepository.Add(newReport, cancellationToken);

            return;
        }

        existing.UpdateCount(existing.TotalCount + contribution);

        await _reportProductMetricsRepository.Update(existing, cancellationToken);
    }
}
