using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Procurement.Application.PurchaseOrders.Commands.CreatePurchaseOrder;
using Invoria.Procurement.Application.PurchaseOrders.Commands.SubmitPurchaseOrder;
using Invoria.Procurement.Contracts.PurchaseOrders;
using Invoria.Procurement.Domain.Parties;
using Invoria.Procurement.Domain.PurchaseOrders;
using Invoria.Procurement.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Procurement.Application.Tests.PurchaseOrders;

[TestFixture]
public class SubmitPurchaseOrderCommandHandlerTests : ProcurementTestFixture
{
    private IProcurementRepository<Supplier> SupplierRepository { get; }
    private IProcurementRepository<PurchaseOrder> PurchaseOrderRepository { get; }
    private IMediator Mediator { get; }

    public SubmitPurchaseOrderCommandHandlerTests()
    {
        SupplierRepository = ServiceProvider.GetRequiredService<IProcurementRepository<Supplier>>();
        PurchaseOrderRepository = ServiceProvider.GetRequiredService<IProcurementRepository<PurchaseOrder>>();
        Mediator = ServiceProvider.GetRequiredService<IMediator>();
    }

    [Test]
    public async Task Should_submit_purchase_order_when_it_is_draft()
    {
        var created = await CreateDraftPurchaseOrderAsync();

        var command = new SubmitPurchaseOrderCommand(created.Id);
        var result = await Mediator.Send(command);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.State.Should().Be(PurchaseState.Submitted);

        var purchaseOrder = await PurchaseOrderRepository.SingleOrDefault(x => x.Id == created.Id);
        purchaseOrder.Should().NotBeNull();
        purchaseOrder!.State.Should().Be(PurchaseState.Submitted);
    }

    [Test]
    public async Task Should_fail_when_purchase_order_not_found()
    {
        var command = new SubmitPurchaseOrderCommand(Guid.NewGuid().ToString("N"));
        var result = await Mediator.Send(command);

        result.ShouldBeFailure(typeof(NotFoundException));
    }

    [Test]
    public async Task Should_fail_when_purchase_order_state_does_not_allow_submit()
    {
        var created = await CreateDraftPurchaseOrderAsync();
        created.Submit();
        await PurchaseOrderRepository.Update(created);

        var command = new SubmitPurchaseOrderCommand(created.Id);
        var result = await Mediator.Send(command);

        result.ShouldBeFailure(typeof(BusinessLogicException));
    }

    [Test]
    public async Task Should_fail_when_purchase_order_has_no_items()
    {
        var supplier = Supplier.Create(
            id: Guid.NewGuid().ToString("N"),
            supplierCode: "SUP-" + Guid.NewGuid().ToString("N")[..8],
            name: "Submit Supplier",
            contactEmail: "submit-empty@example.com",
            phone: "+1",
            createdBy: null);
        await SupplierRepository.Add(supplier);

        var purchaseOrder = new PurchaseOrder(
            id: Guid.NewGuid().ToString("N"),
            purchaseNumber: "PO-" + Guid.NewGuid().ToString("N")[..8],
            supplierId: supplier.Id,
            orderDate: DateTime.UtcNow.Date,
            expectedDeliveryDate: DateTime.UtcNow.Date.AddDays(3),
            createdBy: null);
        await PurchaseOrderRepository.Add(purchaseOrder);

        var command = new SubmitPurchaseOrderCommand(purchaseOrder.Id);
        var result = await Mediator.Send(command);

        result.ShouldBeFailure(typeof(BusinessLogicException));
    }

    private async Task<PurchaseOrder> CreateDraftPurchaseOrderAsync()
    {
        var supplier = Supplier.Create(
            id: Guid.NewGuid().ToString("N"),
            supplierCode: "SUP-" + Guid.NewGuid().ToString("N")[..8],
            name: "Submit Supplier",
            contactEmail: "submit@example.com",
            phone: "+1",
            createdBy: null);
        await SupplierRepository.Add(supplier);

        var createCommand = new CreatePurchaseOrderCommand(
            supplierId: supplier.Id,
            taxAmount: 0m,
            discountAmount: 0m,
            orderDate: DateTime.UtcNow.Date,
            expectedDeliveryDate: DateTime.UtcNow.Date.AddDays(3),
            purchaseOrderItems:
            [
                new CreatePurchaseOrderItemCommand(
                    productId: Guid.NewGuid().ToString("N"),
                    quantity: 1,
                    unitPrice: 100m,
                    supplierProductCode: "SKU-01")
            ]);

        var createResult = await Mediator.Send(createCommand);
        createResult.ShouldBeSuccess();

        var purchaseOrder = await PurchaseOrderRepository.SingleOrDefault(x => x.Id == createResult.Value!.Id);
        purchaseOrder.Should().NotBeNull();
        return purchaseOrder!;
    }
}
