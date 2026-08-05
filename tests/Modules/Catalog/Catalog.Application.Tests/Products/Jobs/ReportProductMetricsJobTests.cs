using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.Catalog.Application.Products.Jobs;
using Invoria.Catalog.Domain;
using Invoria.Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Catalog.Application.Tests.Products.Jobs;

using ReportProductMetricsEntity = Invoria.Catalog.Domain.Products.ReportProductMetrics;

[TestFixture]
public class ReportProductMetricsJobTests : CatalogBackgroundJobTestFixture
{
    private const int ProductCount = 100;

    protected ICatalogRepository<Product> ProductRepository { get; }
    protected ICatalogRepository<ReportProductMetricsEntity> ReportRepository { get; }

    public ReportProductMetricsJobTests()
    {
        ProductRepository = ServiceProvider.GetRequiredService<ICatalogRepository<Product>>();
        ReportRepository = ServiceProvider.GetRequiredService<ICatalogRepository<ReportProductMetricsEntity>>();
    }

    [Test]
    public async Task Should_upsert_product_metrics_per_bucket_for_every_period()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        await SeedProductsAsync(30, now);
        await SeedProductsAsync(20, now.AddDays(-10));
        await SeedProductsAsync(30, now.AddMonths(-3));
        await SeedProductsAsync(20, now.AddYears(-2));

        var job = ServiceProvider.GetRequiredService<ReportProductMetricsJob>();

        // Act
        await job.Execute(CancellationToken.None);

        // Assert
        var products = await ProductRepository.AsQuerable().ToListAsync();

        var expectedDaily = GroupByBucket(products,
            p => new DateTimeOffset(p.CreatedAt.Year, p.CreatedAt.Month, p.CreatedAt.Day, 0, 0, 0, p.CreatedAt.Offset));

        var expectedMonthly = GroupByBucket(products,
            p => new DateTimeOffset(p.CreatedAt.Year, p.CreatedAt.Month, 1, 0, 0, 0, p.CreatedAt.Offset));

        var expectedYearly = GroupByBucket(products,
            p => new DateTimeOffset(p.CreatedAt.Year, 1, 1, 0, 0, 0, p.CreatedAt.Offset));

        var reports = await ReportRepository.AsQuerable().ToListAsync();

        AssertPeriodReports(reports, ReportPeriod.Daily, expectedDaily);
        AssertPeriodReports(reports, ReportPeriod.Monthly, expectedMonthly);
        AssertPeriodReports(reports, ReportPeriod.Yearly, expectedYearly);

        var allTimeReport = reports.Single(x => x.Period == ReportPeriod.AllTheTime);
        allTimeReport.Date.Should().Be(DateTimeOffset.MinValue);
        allTimeReport.TotalCount.Should().Be(products.Count);
        allTimeReport.TotalCount.Should().Be(ProductCount);
    }

    private async Task SeedProductsAsync(int count, DateTimeOffset createdAt)
    {
        for (var i = 0; i < count; i++)
        {
            var product = new Product($"Product {Guid.NewGuid():N}", 100);

            SetCreatedAt(product, createdAt);

            await ProductRepository.Add(product);
        }
    }

    private static void SetCreatedAt(Product product, DateTimeOffset createdAt)
    {
        var property = typeof(AuditedAggregateRoot).GetProperty(nameof(AuditedAggregateRoot.CreatedAt));

        property!.SetValue(product, createdAt);
    }

    private static Dictionary<DateTimeOffset, long> GroupByBucket(
        List<Product> products,
        Func<Product, DateTimeOffset> bucketSelector)
    {
        return products
            .GroupBy(bucketSelector)
            .ToDictionary(g => g.Key, g => g.LongCount());
    }

    private static void AssertPeriodReports(
        List<ReportProductMetricsEntity> reports,
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
