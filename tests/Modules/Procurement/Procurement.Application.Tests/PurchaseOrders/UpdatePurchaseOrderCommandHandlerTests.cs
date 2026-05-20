using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Procurement.Application.PurchaseOrders.Commands.ApprovePurchaseOrder;
using Invoria.Procurement.Application.PurchaseOrders.Commands.CreatePurchaseOrder;
using Invoria.Procurement.Application.PurchaseOrders.Commands.ReopenPurchaseOrder;
using Invoria.Procurement.Application.PurchaseOrders.Commands.SubmitPurchaseOrder;
using Invoria.Procurement.Application.PurchaseOrders.Commands.UpdatePurchaseOrder;
using Invoria.Procurement.Contracts.PurchaseOrders;
using Invoria.Procurement.Domain.Parties;
using Invoria.Procurement.Domain.PurchaseOrders;
using Invoria.Procurement.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Procurement.Application.Tests.PurchaseOrders;

[TestFixture]
public class UpdatePurchaseOrderCommandHandlerTests : ProcurementTestFixture
{
    private IProcurementRepository<Supplier> SupplierRepository { get; }
    private IProcurementRepository<PurchaseOrder> PurchaseOrderRepository { get; }
    private IMediator Mediator { get; }

    public UpdatePurchaseOrderCommandHandlerTests()
    {
        SupplierRepository = ServiceProvider.GetRequiredService<IProcurementRepository<Supplier>>();
        PurchaseOrderRepository = ServiceProvider.GetRequiredService<IProcurementRepository<PurchaseOrder>>();
        Mediator = ServiceProvider.GetRequiredService<IMediator>();
    }

    [Test]
    public async Task Should_update_purchase_order_when_it_is_draft()
    {
        var created = await CreateDraftPurchaseOrderAsync();
        var newSupplier = await CreateSupplierAsync("Update Supplier Draft");

        var command = new UpdatePurchaseOrderCommand(
            id: created.Id,
            supplierId: newSupplier.Id,
            taxAmount: 5m,
            discountAmount: 2m,
            purchaseOrderItems:
            [
                new UpdatePurchaseOrderItemCommand(
                    productId: Guid.NewGuid().ToString("N"),
                    quantity: 2,
                    unitPrice: 50m,
                    supplierProductCode: "SKU-NEW")
            ]);

        var result = await Mediator.Send(command);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.State.Should().Be(PurchaseState.Draft);
        result.Value.SupplierId.Should().Be(newSupplier.Id);
        result.Value.TaxAmount.Should().Be(5m);
        result.Value.DiscountAmount.Should().Be(2m);
        result.Value.SubTotal.Should().Be(100m);
        result.Value.TotalAmount.Should().Be(103m);
        result.Value.PurchaseOrderItems.Should().HaveCount(1);

        var persisted = await PurchaseOrderRepository.SingleOrDefault(x => x.Id == created.Id);
        persisted.Should().NotBeNull();
        persisted!.SupplierId.Should().Be(newSupplier.Id);
        persisted.TaxAmount.Should().Be(5m);
        persisted.DiscountAmount.Should().Be(2m);
        persisted.SubTotal.Should().Be(100m);
        persisted.TotalAmount.Should().Be(103m);
        persisted.Items.Should().HaveCount(1);
    }

    [Test]
    public async Task Should_update_purchase_order_when_it_is_reopened()
    {
        var reopened = await CreateReopenedPurchaseOrderAsync();
        var newSupplier = await CreateSupplierAsync("Update Supplier Reopened");

        var command = new UpdatePurchaseOrderCommand(
            id: reopened.Id,
            supplierId: newSupplier.Id,
            taxAmount: 0m,
            discountAmount: 0m,
            purchaseOrderItems:
            [
                new UpdatePurchaseOrderItemCommand(
                    productId: Guid.NewGuid().ToString("N"),
                    quantity: 1,
                    unitPrice: 10m,
                    supplierProductCode: null)
            ]);

        var result = await Mediator.Send(command);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.State.Should().Be(PurchaseState.Reopened);
        result.Value.SupplierId.Should().Be(newSupplier.Id);
        result.Value.SubTotal.Should().Be(10m);
        result.Value.TotalAmount.Should().Be(10m);

        var persisted = await PurchaseOrderRepository.SingleOrDefault(x => x.Id == reopened.Id);
        persisted.Should().NotBeNull();
        persisted!.State.Should().Be(PurchaseState.Reopened);
        persisted.Items.Should().HaveCount(1);
    }

    [Test]
    public async Task Should_fail_when_purchase_order_not_found()
    {
        var supplier = await CreateSupplierAsync("Update Supplier NotFound");

        var command = new UpdatePurchaseOrderCommand(
            id: Guid.NewGuid().ToString("N"),
            supplierId: supplier.Id,
            taxAmount: 0m,
            discountAmount: 0m,
            purchaseOrderItems:
            [
                new UpdatePurchaseOrderItemCommand(
                    productId: Guid.NewGuid().ToString("N"),
                    quantity: 1,
                    unitPrice: 10m,
                    supplierProductCode: null)
            ]);

        var result = await Mediator.Send(command);
        result.ShouldBeFailure(typeof(NotFoundException));
    }

    [Test]
    public async Task Should_fail_when_purchase_order_state_does_not_allow_update()
    {
        var submitted = await CreateSubmittedPurchaseOrderAsync();
        var supplier = await CreateSupplierAsync("Update Supplier InvalidState");

        var command = new UpdatePurchaseOrderCommand(
            id: submitted.Id,
            supplierId: supplier.Id,
            taxAmount: 0m,
            discountAmount: 0m,
            purchaseOrderItems:
            [
                new UpdatePurchaseOrderItemCommand(
                    productId: Guid.NewGuid().ToString("N"),
                    quantity: 1,
                    unitPrice: 10m,
                    supplierProductCode: null)
            ]);

        var result = await Mediator.Send(command);
        result.ShouldBeFailure(typeof(BusinessLogicException));
    }

    private async Task<Supplier> CreateSupplierAsync(string name)
    {
        var supplier = Supplier.Create(
            id: Guid.NewGuid().ToString("N"),
            supplierCode: "SUP-" + Guid.NewGuid().ToString("N")[..8],
            name: name,
            contactEmail: "supplier@example.com",
            phone: "+1",
            createdBy: null);
        return await SupplierRepository.Add(supplier);
    }

    private async Task<PurchaseOrder> CreateDraftPurchaseOrderAsync()
    {
        var supplier = await CreateSupplierAsync("Draft supplier for update");

        var createCommand = new CreatePurchaseOrderCommand(
            supplierId: supplier.Id,
            taxAmount: 0m,
            discountAmount: 0m,
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
        purchaseOrder!.State.Should().Be(PurchaseState.Draft);
        return purchaseOrder;
    }

    private async Task<PurchaseOrder> CreateSubmittedPurchaseOrderAsync()
    {
        var draft = await CreateDraftPurchaseOrderAsync();
        var submitCommand = new SubmitPurchaseOrderCommand(draft.Id);
        var submitResult = await Mediator.Send(submitCommand);
        submitResult.ShouldBeSuccess();

        var submitted = await PurchaseOrderRepository.SingleOrDefault(x => x.Id == draft.Id);
        submitted.Should().NotBeNull();
        submitted!.State.Should().Be(PurchaseState.Submitted);
        return submitted;
    }

    private async Task<PurchaseOrder> CreateReopenedPurchaseOrderAsync()
    {
        var submitted = await CreateSubmittedPurchaseOrderAsync();

        var approveCommand = new ApprovePurchaseOrderCommand(submitted.Id);
        var approveResult = await Mediator.Send(approveCommand);
        approveResult.ShouldBeSuccess();

        var reopenCommand = new ReopenPurchaseOrderCommand(submitted.Id);
        var reopenResult = await Mediator.Send(reopenCommand);
        reopenResult.ShouldBeSuccess();

        var reopened = await PurchaseOrderRepository.SingleOrDefault(x => x.Id == submitted.Id);
        reopened.Should().NotBeNull();
        reopened!.State.Should().Be(PurchaseState.Reopened);
        return reopened;
    }
}

