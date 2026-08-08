using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.Ordering.Application.Orders.Jobs;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Application.Tests.Orders.Jobs;

using ReportOrderCompletedMetricsEntity = Invoria.Ordering.Domain.Orders.ReportOrderCompletedMetrics;

[TestFixture]
public class ReportOrderCompletedMetricsJobTests : OrderingBackgroundJobTestFixture
{
    private const int CompletedCount = 100;
    private const int NonCompletedCount = 10;

    protected IOrderingRepository<Order> OrderRepository { get; }
    protected IOrderingRepository<OrderStateTransitionHistory> HistoryRepository { get; }
    protected IOrderingRepository<ReportOrderCompletedMetricsEntity> ReportRepository { get; }

    public ReportOrderCompletedMetricsJobTests()
    {
        OrderRepository = ServiceProvider.GetRequiredService<IOrderingRepository<Order>>();
        HistoryRepository = ServiceProvider.GetRequiredService<IOrderingRepository<OrderStateTransitionHistory>>();
        ReportRepository = ServiceProvider.GetRequiredService<IOrderingRepository<ReportOrderCompletedMetricsEntity>>();
    }

    [Test]
    public async Task Should_upsert_order_completed_metrics_per_bucket_for_every_period()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        await SeedCompletedAsync(30, now);
        await SeedCompletedAsync(20, now.AddDays(-10));
        await SeedCompletedAsync(30, now.AddMonths(-3));
        await SeedCompletedAsync(20, now.AddYears(-2));
        await SeedNonCompletedAsync(NonCompletedCount);

        var job = ServiceProvider.GetRequiredService<ReportOrderCompletedMetricsJob>();

        // Act
        await job.Execute(CancellationToken.None);

        // Assert
        var completedTransitions = await LoadCompletedTransitionsAsync();

        var expectedDaily = GroupByBucket(completedTransitions,
            t => new DateTimeOffset(t.ChangedAt.Year, t.ChangedAt.Month, t.ChangedAt.Day, 0, 0, 0, t.ChangedAt.Offset));

        var expectedMonthly = GroupByBucket(completedTransitions,
            t => new DateTimeOffset(t.ChangedAt.Year, t.ChangedAt.Month, 1, 0, 0, 0, t.ChangedAt.Offset));

        var expectedYearly = GroupByBucket(completedTransitions,
            t => new DateTimeOffset(t.ChangedAt.Year, 1, 1, 0, 0, 0, t.ChangedAt.Offset));

        var reports = await ReportRepository.AsQuerable().ToListAsync();

        AssertPeriodReports(reports, ReportPeriod.Daily, expectedDaily);
        AssertPeriodReports(reports, ReportPeriod.Monthly, expectedMonthly);
        AssertPeriodReports(reports, ReportPeriod.Yearly, expectedYearly);

        var allTimeReport = reports.Single(x => x.Period == ReportPeriod.AllTheTime);
        allTimeReport.Date.Should().Be(DateTimeOffset.MinValue);
        allTimeReport.TotalCount.Should().Be(completedTransitions.Count);
        allTimeReport.TotalCount.Should().Be(CompletedCount);
    }

    private async Task<List<OrderStateTransitionHistory>> LoadCompletedTransitionsAsync()
    {
        var transitions = await HistoryRepository.AsQuerable().ToListAsync();

        return transitions
            .Where(t => t.ToStatus == OrderStatus.Completed)
            .ToList();
    }

    private async Task SeedCompletedAsync(int count, DateTimeOffset changedAt)
    {
        var orders = await OrderTestData.PersistRandomOrdersAsync(OrderRepository, count);

        foreach (var order in orders)
        {
            var transition = new OrderStateTransitionHistory(
                Guid.NewGuid().ToString("N"),
                order.Id,
                OrderStatus.Processing,
                OrderStatus.Completed,
                changedAt);

            await HistoryRepository.Add(transition);
        }
    }

    private async Task SeedNonCompletedAsync(int count)
    {
        var orders = await OrderTestData.PersistRandomOrdersAsync(OrderRepository, count);

        foreach (var order in orders)
        {
            var transition = new OrderStateTransitionHistory(
                Guid.NewGuid().ToString("N"),
                order.Id,
                OrderStatus.Processing,
                OrderStatus.Cancelled,
                DateTimeOffset.UtcNow.AddHours(1));

            await HistoryRepository.Add(transition);
        }
    }

    private static Dictionary<DateTimeOffset, long> GroupByBucket(
        List<OrderStateTransitionHistory> transitions,
        Func<OrderStateTransitionHistory, DateTimeOffset> bucketSelector)
    {
        return transitions
            .GroupBy(bucketSelector)
            .ToDictionary(g => g.Key, g => g.LongCount());
    }

    private static void AssertPeriodReports(
        List<ReportOrderCompletedMetricsEntity> reports,
        ReportPeriod period,
        Dictionary<DateTimeOffset, long> expected)
    {
        var periodReports = reports.Where(x => x.Period == period).ToList();

        periodReports.Count.Should().Be(expected.Count);

        foreach (var expectedBucket in expected)
        {
            var report = periodReports.Single(x => x.Date == expectedBucket.Key);

            report.Date.Should().Be(expectedBucket.Key);
            report.TotalCount.Should().Be(expectedBucket.Value);
        }
    }
}