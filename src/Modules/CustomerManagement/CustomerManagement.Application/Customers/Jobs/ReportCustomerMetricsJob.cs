using Invoria.BackgroundJob.Core;
using Invoria.BackgroundJob.Core.Checkpoints;
using Invoria.BackgroundJob.Core.Context;
using Invoria.BackgroundJob.Core.Jobs;
using Invoria.BackgroundJobs.Abstractions.CheckPoints;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.CustomerManagement.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace Invoria.CustomerManagement.Application.Customers.Jobs;

public sealed class ReportCustomerMetricsJob : IJob
{
    public const string Name = "ReportCustomerMetricsJob";

    public static int BatchSize { get; set; } = 100;

    private readonly IJobCheckpointStore _jobCheckpointStore;
    private readonly IJobExecutionContextAccessor _jobExecutionContextAccessor;
    private readonly ICustomerRepository<Customer> _customerRepository;
    private readonly ICustomerRepository<ReportCustomerMetrics> _reportCustomerMetricsRepository;

    public ReportCustomerMetricsJob(
        IJobCheckpointStore jobCheckpointStore,
        IJobExecutionContextAccessor jobExecutionContextAccessor,
        ICustomerRepository<Customer> customerRepository,
        ICustomerRepository<ReportCustomerMetrics> reportCustomerMetricsRepository)
    {
        _jobCheckpointStore = jobCheckpointStore;
        _jobExecutionContextAccessor = jobExecutionContextAccessor;
        _customerRepository = customerRepository;
        _reportCustomerMetricsRepository = reportCustomerMetricsRepository;
    }

    public async Task Execute(CancellationToken cancellationToken = default)
    {
        var reportJobCheckPoint = await RestoreStateAsync(cancellationToken);

        var jobState = reportJobCheckPoint.GetState<ReportJobCheckPoint>()
            ?? new ReportJobCheckPoint();

        bool isBatchFull;

        do
        {
            var customerQuery = _customerRepository.AsQuerable();

            var orderedCustomers = customerQuery
                .OrderBy(x => x.Id)
                .Skip((int)jobState.LastIndex)
                .Take(BatchSize);

            var customers = await orderedCustomers.ToListAsync(cancellationToken);

            if (customers.Count == 0)
            {
                break;
            }

            await UpdateDailyReportAsync(customers, cancellationToken);
            await UpdateMonthlyReportAsync(customers, cancellationToken);
            await UpdateYearlyReportAsync(customers, cancellationToken);
            await UpdateAllTimeReportAsync(customers, cancellationToken);

            var lastCustomer = customers.LastOrDefault();
            if (lastCustomer is not null)
            {
                jobState.LastId = lastCustomer.Id;
            }

            jobState.LastIndex += customers.Count;

            reportJobCheckPoint.SetState(jobState);

            isBatchFull = customers.Count == BatchSize;
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
        List<Customer> customers,
        CancellationToken cancellationToken)
    {
        var dailyGroups = customers.GroupBy(c =>
            new DateTimeOffset(c.CreatedAt.Year, c.CreatedAt.Month, c.CreatedAt.Day, 0, 0, 0, c.CreatedAt.Offset));

        foreach (var group in dailyGroups)
        {
            await UpsertReportAsync(ReportPeriod.Daily, group.Key, group.LongCount(), cancellationToken);
        }
    }

    private async Task UpdateMonthlyReportAsync(
        List<Customer> customers,
        CancellationToken cancellationToken)
    {
        var monthlyGroups = customers.GroupBy(c =>
            new DateTimeOffset(c.CreatedAt.Year, c.CreatedAt.Month, 1, 0, 0, 0, c.CreatedAt.Offset));

        foreach (var group in monthlyGroups)
        {
            await UpsertReportAsync(ReportPeriod.Monthly, group.Key, group.LongCount(), cancellationToken);
        }
    }

    private async Task UpdateYearlyReportAsync(
        List<Customer> customers,
        CancellationToken cancellationToken)
    {
        var yearlyGroups = customers.GroupBy(c =>
            new DateTimeOffset(c.CreatedAt.Year, 1, 1, 0, 0, 0, c.CreatedAt.Offset));

        foreach (var group in yearlyGroups)
        {
            await UpsertReportAsync(ReportPeriod.Yearly, group.Key, group.LongCount(), cancellationToken);
        }
    }

    private async Task UpdateAllTimeReportAsync(
        List<Customer> customers,
        CancellationToken cancellationToken)
    {
        long contribution = customers.Count;

        await UpsertReportAsync(ReportPeriod.AllTheTime, DateTimeOffset.MinValue, contribution, cancellationToken);
    }

    private async Task UpsertReportAsync(
        ReportPeriod period,
        DateTimeOffset date,
        long contribution,
        CancellationToken cancellationToken)
    {
        var existing = await _reportCustomerMetricsRepository
            .SingleOrDefault(x => x.Period == period && x.Date == date, cancellationToken);

        if (existing is null)
        {
            var newReport = new ReportCustomerMetrics(date, contribution, period);

            await _reportCustomerMetricsRepository.Add(newReport, cancellationToken);

            return;
        }

        existing.UpdateCount(existing.TotalCount + contribution);

        await _reportCustomerMetricsRepository.Update(existing, cancellationToken);
    }
}
