using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.Catalog.Application.Products.Jobs;
using Invoria.Catalog.Domain;
using Invoria.Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Catalog.Application.Tests.Products.Jobs;

[TestFixture]
public class ReportProductMetricsJobTests : CatalogBackgroundJobTestFixture
{
    private const int ProductCount = 100;

    protected ICatalogRepository<Product> ProductRepository { get; }
    protected ICatalogRepository<ReportProductMetrics> ReportRepository { get; }

    public ReportProductMetricsJobTests()
    {
        ProductRepository = ServiceProvider.GetRequiredService<ICatalogRepository<Product>>();
        ReportRepository = ServiceProvider.GetRequiredService<ICatalogRepository<ReportProductMetrics>>();
    }

    [Test]
    public async Task Should_upsert_product_metrics_for_every_period()
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

        var expectedDaily = products.Count(p => p.CreatedAt.Date == now.Date);
        var expectedMonthly = products.Count(p => p.CreatedAt.Year == now.Year && p.CreatedAt.Month == now.Month);
        var expectedYearly = products.Count(p => p.CreatedAt.Year == now.Year);
        var expectedAllTime = products.Count;

        var reports = await ReportRepository.AsQuerable().ToListAsync();

        var dailyReport = reports.Single(x => x.Period == ReportPeriod.Daily);
        dailyReport.TotalCount.Should().Be(expectedDaily);

        var monthlyReport = reports.Single(x => x.Period == ReportPeriod.Monthly);
        monthlyReport.TotalCount.Should().Be(expectedMonthly);

        var yearlyReport = reports.Single(x => x.Period == ReportPeriod.Yearly);
        yearlyReport.TotalCount.Should().Be(expectedYearly);

        var allTimeReport = reports.Single(x => x.Period == ReportPeriod.AllTheTime);
        allTimeReport.TotalCount.Should().Be(expectedAllTime);
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
}
