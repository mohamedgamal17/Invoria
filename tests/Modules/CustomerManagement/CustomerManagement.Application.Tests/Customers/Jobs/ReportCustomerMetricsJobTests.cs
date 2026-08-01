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
    public async Task Should_upsert_customer_metrics_for_every_period()
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

        var expectedDaily = customers.Count(c => c.CreatedAt.Date == now.Date);
        var expectedMonthly = customers.Count(c => c.CreatedAt.Year == now.Year && c.CreatedAt.Month == now.Month);
        var expectedYearly = customers.Count(c => c.CreatedAt.Year == now.Year);
        var expectedAllTime = customers.Count;

        var reports = await ReportRepository.AsQuerable().ToListAsync();

        var dailyReport = reports.Single(x => x.Period == ReportPeriod.Daily);
        dailyReport.TotalCount.Should().Be(expectedDaily);

        var monthlyReport = reports.Single(x => x.Period == ReportPeriod.Monthly);
        monthlyReport.TotalCount.Should().Be(expectedMonthly);

        var yearlyReport = reports.Single(x => x.Period == ReportPeriod.Yearly);
        yearlyReport.TotalCount.Should().Be(expectedYearly);

        var allTimeReport = reports.Single(x => x.Period == ReportPeriod.AllTheTime);
        allTimeReport.TotalCount.Should().Be(expectedAllTime);
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
}
