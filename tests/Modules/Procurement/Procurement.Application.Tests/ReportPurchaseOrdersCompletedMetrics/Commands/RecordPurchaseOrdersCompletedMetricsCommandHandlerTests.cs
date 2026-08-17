using Autofac;
using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.Procurement.Application.ReportPurchaseOrdersCompletedMetrics.Commands.RecordPurchaseOrdersCompletedMetrics;
using Invoria.Procurement.Infrastructure.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Invoria.Procurement.Application.Tests.ReportPurchaseOrdersCompletedMetrics.Commands;

using ReportPurchaseOrdersCompletedMetricsEntity = Invoria.Procurement.Domain.PurchaseOrders.ReportPurchaseOrdersCompletedMetrics;

[TestFixture]
public class RecordPurchaseOrdersCompletedMetricsCommandHandlerTests : ReportPurchaseOrdersCompletedMetricsTestFixture
{
    private IMediator Mediator => Scope.Resolve<IMediator>();

    protected override async Task BeforeAnyTestRunAsync()
    {
        await ClearDataAsync();
    }

    private async Task ClearDataAsync()
    {
        var db = Scope.Resolve<ProcurementDbContext>();
        var reports = await db.Set<ReportPurchaseOrdersCompletedMetricsEntity>().ToListAsync();
        db.RemoveRange(reports);
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task Should_record_completed_count_for_every_period()
    {
        var occurredOn = new DateTimeOffset(2024, 5, 7, 10, 30, 0, TimeSpan.Zero);

        var result = await Mediator.Send(new RecordPurchaseOrdersCompletedMetricsCommand
        {
            OccurredOn = occurredOn
        });

        result.ShouldBeSuccess();

        var reports = await Scope.Resolve<ProcurementDbContext>().Set<ReportPurchaseOrdersCompletedMetricsEntity>().ToListAsync();

        reports.Should().HaveCount(4);

        AssertPeriod(reports, ReportPeriod.Daily,
            new DateTimeOffset(2024, 5, 7, 0, 0, 0, TimeSpan.Zero));
        AssertPeriod(reports, ReportPeriod.Monthly,
            new DateTimeOffset(2024, 5, 1, 0, 0, 0, TimeSpan.Zero));
        AssertPeriod(reports, ReportPeriod.Yearly,
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
        AssertPeriod(reports, ReportPeriod.AllTheTime, DateTimeOffset.MinValue);
    }

    [Test]
    public async Task Should_aggregate_multiple_completed_orders_in_same_period()
    {
        var occurredOn = new DateTimeOffset(2024, 5, 7, 10, 30, 0, TimeSpan.Zero);

        await Mediator.Send(new RecordPurchaseOrdersCompletedMetricsCommand
        {
            OccurredOn = occurredOn
        });

        await Mediator.Send(new RecordPurchaseOrdersCompletedMetricsCommand
        {
            OccurredOn = occurredOn
        });

        var reports = await GetReportsAsync();

        reports.Should().HaveCount(4);

        reports.Where(x => x.Period == ReportPeriod.Daily)
            .Single().TotalCount.Should().Be(2);
        reports.Where(x => x.Period == ReportPeriod.Daily)
            .Single().Id.Should()
            .Be(ReportPurchaseOrdersCompletedMetricsEntity.CreateId(
                ReportPeriod.Daily,
                new DateTimeOffset(2024, 5, 7, 0, 0, 0, TimeSpan.Zero)));
        reports.Where(x => x.Period == ReportPeriod.Monthly)
            .Single().TotalCount.Should().Be(2);
        reports.Where(x => x.Period == ReportPeriod.Yearly)
            .Single().TotalCount.Should().Be(2);
        reports.Where(x => x.Period == ReportPeriod.AllTheTime)
            .Single().TotalCount.Should().Be(2);
    }

    [Test]
    public async Task Should_bucket_by_occurred_on_into_distinct_period_rows()
    {
        var mayOccurredOn = new DateTimeOffset(2024, 5, 7, 10, 30, 0, TimeSpan.Zero);
        var juneOccurredOn = new DateTimeOffset(2024, 6, 9, 14, 0, 0, TimeSpan.Zero);

        await Mediator.Send(new RecordPurchaseOrdersCompletedMetricsCommand
        {
            OccurredOn = mayOccurredOn
        });

        await Mediator.Send(new RecordPurchaseOrdersCompletedMetricsCommand
        {
            OccurredOn = juneOccurredOn
        });

        var reports = await GetReportsAsync();

        var daily = reports.Where(x => x.Period == ReportPeriod.Daily).ToList();
        daily.Should().HaveCount(2);
        daily.Single(x => x.Date == new DateTimeOffset(2024, 5, 7, 0, 0, 0, TimeSpan.Zero))
            .TotalCount.Should().Be(1);
        daily.Single(x => x.Date == new DateTimeOffset(2024, 6, 9, 0, 0, 0, TimeSpan.Zero))
            .TotalCount.Should().Be(1);

        var monthly = reports.Where(x => x.Period == ReportPeriod.Monthly).ToList();
        monthly.Should().HaveCount(2);
        monthly.Single(x => x.Date == new DateTimeOffset(2024, 5, 1, 0, 0, 0, TimeSpan.Zero))
            .TotalCount.Should().Be(1);
        monthly.Single(x => x.Date == new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero))
            .TotalCount.Should().Be(1);

        var yearly = reports.Where(x => x.Period == ReportPeriod.Yearly).ToList();
        yearly.Should().HaveCount(1);
        yearly.Single().TotalCount.Should().Be(2);

        reports.Single(x => x.Period == ReportPeriod.AllTheTime)
            .TotalCount.Should().Be(2);
    }

    private async Task<List<ReportPurchaseOrdersCompletedMetricsEntity>> GetReportsAsync()
    {
        return await Scope.Resolve<ProcurementDbContext>()
            .Set<ReportPurchaseOrdersCompletedMetricsEntity>()
            .ToListAsync();
    }

    private static void AssertPeriod(
        List<ReportPurchaseOrdersCompletedMetricsEntity> reports,
        ReportPeriod period,
        DateTimeOffset expectedDate)
    {
        var report = reports.Single(x => x.Period == period);

        report.Date.Should().Be(expectedDate);
        report.TotalCount.Should().Be(1);
    }
}