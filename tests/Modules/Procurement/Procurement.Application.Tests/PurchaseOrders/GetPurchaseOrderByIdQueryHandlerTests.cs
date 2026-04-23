using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Procurement.Application.PurchaseOrders.Queries.GetPurchaseOrderById;
using Invoria.Procurement.Domain.Parties;
using Invoria.Procurement.Domain.PurchaseOrders;
using Invoria.Procurement.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Procurement.Application.Tests.PurchaseOrders;

[TestFixture]
public class GetPurchaseOrderByIdQueryHandlerTests : ProcurementTestFixture
{
    private IProcurementRepository<Supplier> SupplierRepository { get; }
    private IProcurementRepository<PurchaseOrder> PurchaseOrderRepository { get; }
    private IMediator Mediator { get; }

    public GetPurchaseOrderByIdQueryHandlerTests()
    {
        SupplierRepository = ServiceProvider.GetRequiredService<IProcurementRepository<Supplier>>();
        PurchaseOrderRepository = ServiceProvider.GetRequiredService<IProcurementRepository<PurchaseOrder>>();
        Mediator = ServiceProvider.GetRequiredService<IMediator>();
    }

    [Test]
    public async Task Should_return_purchase_order_when_found()
    {
        var purchaseOrder = await CreatePurchaseOrderAsync("PO-GET-001");

        var query = new GetPurchaseOrderByIdQuery { Id = purchaseOrder.Id };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(purchaseOrder.Id);
        result.Value.PurchaseNumber.Should().Be(purchaseOrder.PurchaseNumber);
        result.Value.SupplierId.Should().Be(purchaseOrder.SupplierId);
        result.Value.Supplier.Should().NotBeNull();
        result.Value.Supplier!.Id.Should().Be(purchaseOrder.SupplierId);
        result.Value.Supplier.Name.Should().Be("Supplier A1B2C3");
        result.Value.Supplier.SupplierCode.Should().StartWith("SUP-");
        result.Value.PurchaseOrderItems.Should().HaveCount(1);
        result.Value.StateHistory.Should().NotBeNull();
    }

    [Test]
    public async Task Should_return_state_history_by_default()
    {
        var purchaseOrder = await CreatePurchaseOrderAsync(
            "PO-GET-HISTORY-001",
            applyTransitions: x =>
            {
                x.Submit();
                x.Reject("Supplier failed validation");
            });

        var query = new GetPurchaseOrderByIdQuery { Id = purchaseOrder.Id };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.StateHistory.Should().HaveCount(2);
        result.Value.StateHistory[0].FromState.Should().Be(Invoria.Procurement.Contracts.PurchaseOrders.PurchaseState.Draft);
        result.Value.StateHistory[0].ToState.Should().Be(Invoria.Procurement.Contracts.PurchaseOrders.PurchaseState.Submitted);
        result.Value.StateHistory[1].FromState.Should().Be(Invoria.Procurement.Contracts.PurchaseOrders.PurchaseState.Submitted);
        result.Value.StateHistory[1].ToState.Should().Be(Invoria.Procurement.Contracts.PurchaseOrders.PurchaseState.Rejected);
        result.Value.StateHistory[1].Reason.Should().Be("Supplier failed validation");
        result.Value.StateHistory[1].ChangedAt.Should().BeOnOrAfter(result.Value.StateHistory[0].ChangedAt);
    }

    [Test]
    public async Task Should_return_failure_when_purchase_order_not_found()
    {
        var query = new GetPurchaseOrderByIdQuery { Id = Guid.NewGuid().ToString("N") };

        var result = await Mediator.Send(query);

        result.IsSuccess.Should().BeFalse();
        result.Exception.Should().NotBeNull();
        result.Exception.Should().BeOfType<NotFoundException>();
    }

    private async Task<PurchaseOrder> CreatePurchaseOrderAsync(
        string purchaseNumber,
        Action<PurchaseOrder>? applyTransitions = null)
    {
        var supplier = Supplier.Create(
            id: Guid.NewGuid().ToString("N"),
            supplierCode: "SUP-" + Guid.NewGuid().ToString("N")[..8],
            name: "Supplier A1B2C3",
            contactEmail: null,
            phone: null,
            createdBy: "tests");
        await SupplierRepository.Add(supplier);

        var purchaseOrder = new PurchaseOrder(
            id: Guid.NewGuid().ToString("N"),
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
}
