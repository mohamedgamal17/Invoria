using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.CustomerManagement.Application.Customers.Jobs;
using Invoria.CustomerManagement.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.CustomerManagement.Application.Tests.Customers.Jobs;

[TestFixture]
public class ReportCustomerMetricsJobTests : CustomerBackgroundJobTestFixture
{
    private const int CustomerCount = 100;

    protected ICustomerRepository<Customer> CustomerRepository { get; }
    protected ICustomerRepository<ReportCustomerMetrics> ReportRepository { get; }

    public ReportCustomerMetricsJobTests()
    {
        CustomerRepository = ServiceProvider.GetRequiredService<ICustomerRepository<Customer>>();
        ReportRepository = ServiceProvider.GetRequiredService<ICustomerRepository<ReportCustomerMetrics>>();
    }

    [Test]
    public async Task Should_upsert_customer_metrics_per_bucket_for_every_period()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        await SeedCustomersAsync(30, now);
        await SeedCustomersAsync(20, now.AddDays(-10));
        await SeedCustomersAsync(30, now.AddMonths(-3));
        await SeedCustomersAsync(20, now.AddYears(-2));

        var job = ServiceProvider.GetRequiredService<ReportCustomerMetricsJob>();

        // Act
        await job.Execute(CancellationToken.None);

        // Assert
        var customers = await CustomerRepository.AsQuerable().ToListAsync();

        var expectedDaily = GroupByBucket(customers,
            c => new DateTimeOffset(c.CreatedAt.Year, c.CreatedAt.Month, c.CreatedAt.Day, 0, 0, 0, c.CreatedAt.Offset));

        var expectedMonthly = GroupByBucket(customers,
            c => new DateTimeOffset(c.CreatedAt.Year, c.CreatedAt.Month, 1, 0, 0, 0, c.CreatedAt.Offset));

        var expectedYearly = GroupByBucket(customers,
            c => new DateTimeOffset(c.CreatedAt.Year, 1, 1, 0, 0, 0, c.CreatedAt.Offset));

        var reports = await ReportRepository.AsQuerable().ToListAsync();

        AssertPeriodReports(reports, ReportPeriod.Daily, expectedDaily);
        AssertPeriodReports(reports, ReportPeriod.Monthly, expectedMonthly);
        AssertPeriodReports(reports, ReportPeriod.Yearly, expectedYearly);

        var allTimeReport = reports.Single(x => x.Period == ReportPeriod.AllTheTime);
        allTimeReport.Date.Should().Be(DateTimeOffset.MinValue);
        allTimeReport.TotalCount.Should().Be(customers.Count);
        allTimeReport.TotalCount.Should().Be(CustomerCount);
    }

    private async Task SeedCustomersAsync(int count, DateTimeOffset createdAt)
    {
        for (var i = 0; i < count; i++)
        {
            var customer = new Customer($"Customer {Guid.NewGuid():N}");

            SetCreatedAt(customer, createdAt);

            await CustomerRepository.Add(customer);
        }
    }

    private static void SetCreatedAt(Customer customer, DateTimeOffset createdAt)
    {
        var property = typeof(AuditedAggregateRoot).GetProperty(nameof(AuditedAggregateRoot.CreatedAt));

        property!.SetValue(customer, createdAt);
    }

    private static Dictionary<DateTimeOffset, long> GroupByBucket(
        List<Customer> customers,
        Func<Customer, DateTimeOffset> bucketSelector)
    {
        return customers
            .GroupBy(bucketSelector)
            .ToDictionary(g => g.Key, g => g.LongCount());
    }

    private static void AssertPeriodReports(
        List<ReportCustomerMetrics> reports,
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
