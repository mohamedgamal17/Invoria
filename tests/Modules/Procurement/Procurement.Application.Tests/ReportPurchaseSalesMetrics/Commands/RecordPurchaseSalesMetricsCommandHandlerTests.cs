using Autofac;
using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Enums;
using Invoria.Procurement.Application.PurchaseOrders.Commands.ApprovePurchaseOrder;
using Invoria.Procurement.Application.PurchaseOrders.Commands.CompletePurchaseOrder;
using Invoria.Procurement.Application.PurchaseOrders.Commands.CreatePurchaseOrder;
using Invoria.Procurement.Application.PurchaseOrders.Commands.SubmitPurchaseOrder;
using Invoria.Procurement.Application.ReportPurchaseSalesMetrics.Commands.RecordPurchaseSalesMetrics;
using Invoria.Procurement.Contracts.PurchaseOrders;
using Invoria.Procurement.Domain.Parties;
using Invoria.Procurement.Domain.PurchaseOrders;
using Invoria.Procurement.Domain.Repositories;
using Invoria.Procurement.Infrastructure.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Invoria.Procurement.Application.Tests.ReportPurchaseSalesMetrics.Commands;

using ReportPurchaseSalesMetricsEntity = Invoria.Procurement.Domain.PurchaseOrders.ReportPurchaseSalesMetrics;

[TestFixture]
public class RecordPurchaseSalesMetricsCommandHandlerTests : ReportPurchaseSalesMetricsTestFixture
{
    private IMediator Mediator => Scope.Resolve<IMediator>();
    private IProcurementRepository<PurchaseOrder> PurchaseOrderRepository =>
        Scope.Resolve<IProcurementRepository<PurchaseOrder>>();
    private IProcurementRepository<Supplier> SupplierRepository =>
        Scope.Resolve<IProcurementRepository<Supplier>>();

    protected override async Task BeforeAnyTestRunAsync()
    {
        await ClearDataAsync();
    }

    private async Task ClearDataAsync()
    {
        var db = Scope.Resolve<ProcurementDbContext>();
        var reports = await db.Set<ReportPurchaseSalesMetricsEntity>().ToListAsync();
        db.RemoveRange(reports);
        var purchaseOrders = await db.Set<PurchaseOrder>().ToListAsync();
        db.RemoveRange(purchaseOrders);
        var suppliers = await db.Set<Supplier>().ToListAsync();
        db.RemoveRange(suppliers);
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task Should_record_purchase_sales_metrics_for_every_period()
    {
        var purchaseOrder = await SeedCompletedPurchaseOrderAsync();
        var occurredOn = new DateTimeOffset(2024, 5, 7, 10, 30, 0, TimeSpan.Zero);

        var result = await Mediator.Send(new RecordPurchaseSalesMetricsCommand
        {
            PurchaseOrderId = purchaseOrder.Id,
            OccurredOn = occurredOn
        });

        result.ShouldBeSuccess();

        var expected = await LoadContributionAsync(purchaseOrder.Id);

        var reports = await GetReportsAsync();

        reports.Should().HaveCount(4);

        AssertPeriod(reports, ReportPeriod.Daily,
            new DateTimeOffset(2024, 5, 7, 0, 0, 0, TimeSpan.Zero), expected);
        AssertPeriod(reports, ReportPeriod.Monthly,
            new DateTimeOffset(2024, 5, 1, 0, 0, 0, TimeSpan.Zero), expected);
        AssertPeriod(reports, ReportPeriod.Yearly,
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero), expected);
        AssertPeriod(reports, ReportPeriod.AllTheTime, DateTimeOffset.MinValue, expected);
    }

    [Test]
    public async Task Should_aggregate_multiple_purchase_orders_in_same_period()
    {
        var purchaseOrder1 = await SeedCompletedPurchaseOrderAsync();
        var purchaseOrder2 = await SeedCompletedPurchaseOrderAsync();
        var occurredOn = new DateTimeOffset(2024, 5, 7, 10, 30, 0, TimeSpan.Zero);

        await Mediator.Send(new RecordPurchaseSalesMetricsCommand
        {
            PurchaseOrderId = purchaseOrder1.Id,
            OccurredOn = occurredOn
        });

        await Mediator.Send(new RecordPurchaseSalesMetricsCommand
        {
            PurchaseOrderId = purchaseOrder2.Id,
            OccurredOn = occurredOn
        });

        var expected1 = await LoadContributionAsync(purchaseOrder1.Id);
        var expected2 = await LoadContributionAsync(purchaseOrder2.Id);
        var combined = new SalesContribution(
            expected1.TotalAmount + expected2.TotalAmount,
            expected1.SubTotal + expected2.SubTotal);

        var reports = await GetReportsAsync();

        reports.Should().HaveCount(4);

        AssertPeriod(reports, ReportPeriod.Daily,
            new DateTimeOffset(2024, 5, 7, 0, 0, 0, TimeSpan.Zero), combined);
        AssertPeriod(reports, ReportPeriod.Monthly,
            new DateTimeOffset(2024, 5, 1, 0, 0, 0, TimeSpan.Zero), combined);
        AssertPeriod(reports, ReportPeriod.Yearly,
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero), combined);
        AssertPeriod(reports, ReportPeriod.AllTheTime, DateTimeOffset.MinValue, combined);
    }

    [Test]
    public async Task Should_bucket_by_occurred_on_into_distinct_period_rows()
    {
        var purchaseOrder1 = await SeedCompletedPurchaseOrderAsync();
        var purchaseOrder2 = await SeedCompletedPurchaseOrderAsync();
        var mayOccurredOn = new DateTimeOffset(2024, 5, 7, 10, 30, 0, TimeSpan.Zero);
        var juneOccurredOn = new DateTimeOffset(2024, 6, 9, 14, 0, 0, TimeSpan.Zero);

        await Mediator.Send(new RecordPurchaseSalesMetricsCommand
        {
            PurchaseOrderId = purchaseOrder1.Id,
            OccurredOn = mayOccurredOn
        });

        await Mediator.Send(new RecordPurchaseSalesMetricsCommand
        {
            PurchaseOrderId = purchaseOrder2.Id,
            OccurredOn = juneOccurredOn
        });

        var expected1 = await LoadContributionAsync(purchaseOrder1.Id);
        var expected2 = await LoadContributionAsync(purchaseOrder2.Id);

        var reports = await GetReportsAsync();

        var daily = reports.Where(x => x.Period == ReportPeriod.Daily).ToList();
        daily.Should().HaveCount(2);
        AssertPeriodReport(daily.Single(x => x.Date == new DateTimeOffset(2024, 5, 7, 0, 0, 0, TimeSpan.Zero)), expected1);
        AssertPeriodReport(daily.Single(x => x.Date == new DateTimeOffset(2024, 6, 9, 0, 0, 0, TimeSpan.Zero)), expected2);

        var monthly = reports.Where(x => x.Period == ReportPeriod.Monthly).ToList();
        monthly.Should().HaveCount(2);
        AssertPeriodReport(monthly.Single(x => x.Date == new DateTimeOffset(2024, 5, 1, 0, 0, 0, TimeSpan.Zero)), expected1);
        AssertPeriodReport(monthly.Single(x => x.Date == new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero)), expected2);

        var yearly = reports.Where(x => x.Period == ReportPeriod.Yearly).ToList();
        yearly.Should().HaveCount(1);
        AssertPeriodReport(yearly.Single(), new SalesContribution(
            expected1.TotalAmount + expected2.TotalAmount,
            expected1.SubTotal + expected2.SubTotal));

        var allTime = reports.Single(x => x.Period == ReportPeriod.AllTheTime);
        allTime.TotalAmount.Should().Be(expected1.TotalAmount + expected2.TotalAmount);
        allTime.SubTotal.Should().Be(expected1.SubTotal + expected2.SubTotal);
    }

    private async Task<PurchaseOrder> SeedCompletedPurchaseOrderAsync()
    {
        var supplier = Supplier.Create(
            id: Guid.NewGuid().ToString("N"),
            supplierCode: "SUP-" + Guid.NewGuid().ToString("N")[..8],
            name: "Sales Supplier",
            contactEmail: "sales@example.com",
            phone: "+1",
            createdBy: null);
        await SupplierRepository.Add(supplier);

        var createCommand = new CreatePurchaseOrderCommand(
            supplierId: supplier.Id,
            purchaseOrderItems:
            [
                new CreatePurchaseOrderItemCommand(
                    productId: Guid.NewGuid().ToString("N"),
                    quantity: 3,
                    unitPrice: 100m)
            ]);

        var createResult = await Mediator.Send(createCommand);
        createResult.ShouldBeSuccess();

        var purchaseOrder = await PurchaseOrderRepository.SingleOrDefault(x => x.Id == createResult.Value!.Id);
        purchaseOrder.Should().NotBeNull();
        purchaseOrder!.State.Should().Be(PurchaseState.Draft);

        await Mediator.Send(new SubmitPurchaseOrderCommand(purchaseOrder.Id));
        await Mediator.Send(new ApprovePurchaseOrderCommand(purchaseOrder.Id));

        var completeResult = await Mediator.Send(new CompletePurchaseOrderCommand(purchaseOrder.Id));
        completeResult.ShouldBeSuccess();

        var completed = await PurchaseOrderRepository.SingleOrDefault(x => x.Id == purchaseOrder.Id);
        completed.Should().NotBeNull();
        completed!.State.Should().Be(PurchaseState.Completed);
        return completed;
    }

    private async Task<SalesContribution> LoadContributionAsync(string purchaseOrderId)
    {
        var purchaseOrder = await PurchaseOrderRepository.SingleOrDefault(x => x.Id == purchaseOrderId);

        return new SalesContribution(
            purchaseOrder!.TotalAmount,
            purchaseOrder.SubTotal);
    }

    private async Task<List<ReportPurchaseSalesMetricsEntity>> GetReportsAsync()
    {
        return await Scope.Resolve<ProcurementDbContext>()
            .Set<ReportPurchaseSalesMetricsEntity>()
            .ToListAsync();
    }

    private static void AssertPeriod(
        List<ReportPurchaseSalesMetricsEntity> reports,
        ReportPeriod period,
        DateTimeOffset expectedDate,
        SalesContribution expected)
    {
        var report = reports.Single(x => x.Period == period);

        report.Date.Should().Be(expectedDate);
        AssertPeriodReport(report, expected);
    }

    private static void AssertPeriodReport(ReportPurchaseSalesMetricsEntity report, SalesContribution expected)
    {
        report.TotalAmount.Should().Be(expected.TotalAmount);
        report.SubTotal.Should().Be(expected.SubTotal);
    }

    private readonly record struct SalesContribution(
        decimal TotalAmount,
        decimal SubTotal);
}