using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.Procurement.Application.PurchaseOrders.Queries.ListPurchaseOrders;
using Invoria.Procurement.Domain.Parties;
using Invoria.Procurement.Domain.PurchaseOrders;
using Invoria.Procurement.Domain.Repositories;
using Invoria.Procurement.Infrastructure.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Procurement.Application.Tests.PurchaseOrders;

[TestFixture]
public class ListPurchaseOrdersQueryHandlerTests : ProcurementTestFixture
{
    private IProcurementRepository<Supplier> SupplierRepository { get; }
    private IProcurementRepository<PurchaseOrder> PurchaseOrderRepository { get; }
    private IMediator Mediator { get; }

    public ListPurchaseOrdersQueryHandlerTests()
    {
        SupplierRepository = ServiceProvider.GetRequiredService<IProcurementRepository<Supplier>>();
        PurchaseOrderRepository = ServiceProvider.GetRequiredService<IProcurementRepository<PurchaseOrder>>();
        Mediator = ServiceProvider.GetRequiredService<IMediator>();
    }

    protected override async Task BeforeAnyTestRunAsync()
    {
        await ClearPurchaseOrdersAndSuppliersAsync();
    }

    [Test]
    public async Task Should_return_paged_purchase_orders()
    {
        await CreatePurchaseOrderAsync("PO-A-001");
        await CreatePurchaseOrderAsync("PO-B-001");
        await CreatePurchaseOrderAsync("PO-C-001");

        var query = new ListPurchaseOrdersQuery
        {
            Skip = 1,
            Length = 1
        };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.Info.Skip.Should().Be(1);
        result.Value.Info.Length.Should().Be(1);
        result.Value.Info.TotalCount.Should().Be(3);
        result.Value.Data.Should().ContainSingle();
    }

    [Test]
    public async Task Should_filter_by_number_contains_case_insensitive()
    {
        var matching = await CreatePurchaseOrderAsync("PO-FLTR-ALPHA-001");
        await CreatePurchaseOrderAsync("PO-BETA-002");

        var query = new ListPurchaseOrdersQuery
        {
            Skip = 0,
            Length = 10,
            Number = "fltr-alpha"
        };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.Info.TotalCount.Should().Be(1);
        result.Value.Data.Should().ContainSingle();
        result.Value.Data.Single().Id.Should().Be(matching.Id);
    }

    [Test]
    public async Task Should_ignore_whitespace_only_number_filter()
    {
        var one = await CreatePurchaseOrderAsync("PO-ONE-001");
        var two = await CreatePurchaseOrderAsync("PO-TWO-002");

        var query = new ListPurchaseOrdersQuery
        {
            Skip = 0,
            Length = 10,
            Number = "   "
        };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.Info.TotalCount.Should().Be(2);
        result.Value.Data.Should().HaveCount(2);
        result.Value.Data.Select(x => x.Id).Should().Contain([one.Id, two.Id]);
    }

    [Test]
    public async Task Should_use_default_false_include_flags_without_regression()
    {
        var purchaseOrder = await CreatePurchaseOrderAsync("PO-NOINC-001");

        var query = new ListPurchaseOrdersQuery
        {
            Skip = 0,
            Length = 10
        };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        query.IncludePurchaseItems.Should().BeFalse();
        query.IncludeSupplier.Should().BeFalse();
        var dto = result.Value!.Data.Single(x => x.Id == purchaseOrder.Id);
        dto.Supplier.Should().BeNull();
        dto.StateHistory.Should().NotBeNull();
    }

    [Test]
    public async Task Should_include_state_history_by_default()
    {
        var purchaseOrder = await CreatePurchaseOrderAsync(
            "PO-HISTORY-001",
            applyTransitions: x =>
            {
                x.Submit();
                x.Reject("Missing compliance document");
            });

        var query = new ListPurchaseOrdersQuery
        {
            Skip = 0,
            Length = 10
        };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        var dto = result.Value!.Data.Single(x => x.Id == purchaseOrder.Id);
        dto.StateHistory.Should().HaveCount(2);
        dto.StateHistory[0].FromState.Should().Be(Invoria.Procurement.Contracts.PurchaseOrders.PurchaseState.Draft);
        dto.StateHistory[0].ToState.Should().Be(Invoria.Procurement.Contracts.PurchaseOrders.PurchaseState.Submitted);
        dto.StateHistory[0].Reason.Should().BeNull();
        dto.StateHistory[0].ChangedAt.Should().NotBe(default);
        dto.StateHistory[1].FromState.Should().Be(Invoria.Procurement.Contracts.PurchaseOrders.PurchaseState.Submitted);
        dto.StateHistory[1].ToState.Should().Be(Invoria.Procurement.Contracts.PurchaseOrders.PurchaseState.Rejected);
        dto.StateHistory[1].Reason.Should().Be("Missing compliance document");
        dto.StateHistory[1].ChangedAt.Should().BeOnOrAfter(dto.StateHistory[0].ChangedAt);
    }

    [Test]
    public async Task Should_include_purchase_items_when_indicator_is_true()
    {
        var purchaseOrder = await CreatePurchaseOrderAsync("PO-INCITEMS-001");

        var query = new ListPurchaseOrdersQuery
        {
            Skip = 0,
            Length = 10,
            IncludePurchaseItems = true
        };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        var dto = result.Value!.Data.Single(x => x.Id == purchaseOrder.Id);
        dto.PurchaseOrderItems.Should().ContainSingle();
    }

    [Test]
    public async Task Should_accept_include_supplier_indicator_without_regression()
    {
        var purchaseOrder = await CreatePurchaseOrderAsync("PO-INCSUP-001");

        var query = new ListPurchaseOrdersQuery
        {
            Skip = 0,
            Length = 10,
            IncludeSupplier = true
        };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        var dto = result.Value!.Data.Single(x => x.Id == purchaseOrder.Id);
        dto.Supplier.Should().NotBeNull();
        dto.Supplier!.Id.Should().Be(purchaseOrder.SupplierId);
        dto.Supplier.Name.Should().Be("List Supplier");
        dto.Supplier.SupplierCode.Should().StartWith("SUP-");
    }

    [Test]
    public async Task Should_return_purchase_orders_ordered_by_id_descending()
    {
        var highId = "ffffffffffffffffffffffffffffffff";
        var lowId = "00000000000000000000000000000001";
        await CreatePurchaseOrderAsync("PO-ORD-HIGH", highId);
        await CreatePurchaseOrderAsync("PO-ORD-LOW", lowId);

        var query = new ListPurchaseOrdersQuery
        {
            Skip = 0,
            Length = 10
        };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.Data.Take(2).Select(x => x.Id).Should().Equal(highId, lowId);
    }

    private async Task<PurchaseOrder> CreatePurchaseOrderAsync(
        string purchaseNumber,
        string? id = null,
        Action<PurchaseOrder>? applyTransitions = null)
    {
        var supplier = Supplier.Create(
            id: Guid.NewGuid().ToString("N"),
            supplierCode: "SUP-" + Guid.NewGuid().ToString("N")[..8],
            name: "List Supplier",
            contactEmail: null,
            phone: null,
            createdBy: "tests");
        await SupplierRepository.Add(supplier);

        var purchaseOrder = new PurchaseOrder(
            id: id ?? Guid.NewGuid().ToString("N"),
            purchaseNumber: purchaseNumber,
            supplierId: supplier.Id,
            orderDate: DateTime.UtcNow.Date,
            expectedDeliveryDate: DateTime.UtcNow.Date.AddDays(7));

        purchaseOrder.AddItem(new PurchaseOrderItem(
            id: Guid.NewGuid().ToString("N"),
            purchaseOrderId: purchaseOrder.Id,
            productId: Guid.NewGuid().ToString("N"),
            quantity: 2,
            unitPrice: 100m,
            supplierProductCode: "SKU-01"));

        applyTransitions?.Invoke(purchaseOrder);

        return await PurchaseOrderRepository.Add(purchaseOrder);
    }

    private async Task ClearPurchaseOrdersAndSuppliersAsync()
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ProcurementDbContext>();

        var purchaseOrders = await db.Set<PurchaseOrder>().ToListAsync();
        db.RemoveRange(purchaseOrders);

        var suppliers = await db.Set<Supplier>().ToListAsync();
        db.RemoveRange(suppliers);
        await db.SaveChangesAsync();
    }
}
