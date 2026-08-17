using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.Procurement.Application.Parties.Jobs;
using Invoria.Procurement.Domain.Parties;
using Invoria.Procurement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Procurement.Application.Tests.Parties.Jobs;

using ReportSupplierMetricsEntity = Invoria.Procurement.Domain.Parties.ReportSupplierMetrics;

[TestFixture]
public class ReportSupplierMetricsJobTests : ProcurementBackgroundJobTestFixture
{
    private const int SupplierCount = 100;

    protected IProcurementRepository<Supplier> SupplierRepository { get; }
    protected IProcurementRepository<ReportSupplierMetricsEntity> ReportRepository { get; }

    public ReportSupplierMetricsJobTests()
    {
        SupplierRepository = ServiceProvider.GetRequiredService<IProcurementRepository<Supplier>>();
        ReportRepository = ServiceProvider.GetRequiredService<IProcurementRepository<ReportSupplierMetricsEntity>>();
    }

    [Test]
    public async Task Should_upsert_supplier_metrics_per_bucket_for_every_period()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        await SeedSuppliersAsync(30, now);
        await SeedSuppliersAsync(20, now.AddDays(-10));
        await SeedSuppliersAsync(30, now.AddMonths(-3));
        await SeedSuppliersAsync(20, now.AddYears(-2));

        var job = ServiceProvider.GetRequiredService<ReportSupplierMetricsJob>();

        // Act
        await job.Execute(CancellationToken.None);

        // Assert
        var suppliers = await SupplierRepository.AsQuerable().ToListAsync();

        var expectedDaily = GroupByBucket(suppliers,
            s => new DateTimeOffset(s.CreatedAt.Year, s.CreatedAt.Month, s.CreatedAt.Day, 0, 0, 0, s.CreatedAt.Offset));

        var expectedMonthly = GroupByBucket(suppliers,
            s => new DateTimeOffset(s.CreatedAt.Year, s.CreatedAt.Month, 1, 0, 0, 0, s.CreatedAt.Offset));

        var expectedYearly = GroupByBucket(suppliers,
            s => new DateTimeOffset(s.CreatedAt.Year, 1, 1, 0, 0, 0, s.CreatedAt.Offset));

        var reports = await ReportRepository.AsQuerable().ToListAsync();

        AssertPeriodReports(reports, ReportPeriod.Daily, expectedDaily);
        AssertPeriodReports(reports, ReportPeriod.Monthly, expectedMonthly);
        AssertPeriodReports(reports, ReportPeriod.Yearly, expectedYearly);

        var allTimeReport = reports.Single(x => x.Period == ReportPeriod.AllTheTime);
        allTimeReport.Date.Should().Be(DateTimeOffset.MinValue);
        allTimeReport.TotalCount.Should().Be(suppliers.Count);
        allTimeReport.TotalCount.Should().Be(SupplierCount);
    }

    private async Task SeedSuppliersAsync(int count, DateTimeOffset createdAt)
    {
        for (var i = 0; i < count; i++)
        {
            var supplier = Supplier.Create(
                id: Guid.NewGuid().ToString("N"),
                supplierCode: "SUP-" + Guid.NewGuid().ToString("N")[..8],
                name: "Supplier " + Guid.NewGuid().ToString("N"),
                contactEmail: null,
                phone: null,
                createdBy: "tests");

            SetCreatedAt(supplier, createdAt);

            await SupplierRepository.Add(supplier);
        }
    }

    private static void SetCreatedAt(Supplier supplier, DateTimeOffset createdAt)
    {
        var property = typeof(AuditedAggregateRoot).GetProperty(nameof(AuditedAggregateRoot.CreatedAt));

        property!.SetValue(supplier, createdAt);
    }

    private static Dictionary<DateTimeOffset, long> GroupByBucket(
        List<Supplier> suppliers,
        Func<Supplier, DateTimeOffset> bucketSelector)
    {
        return suppliers
            .GroupBy(bucketSelector)
            .ToDictionary(g => g.Key, g => g.LongCount());
    }

    private static void AssertPeriodReports(
        List<ReportSupplierMetricsEntity> reports,
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