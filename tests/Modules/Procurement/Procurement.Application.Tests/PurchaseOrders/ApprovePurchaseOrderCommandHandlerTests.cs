using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Procurement.Application.PurchaseOrders.Commands.CreatePurchaseOrder;
using Invoria.Procurement.Application.PurchaseOrders.Commands.SubmitPurchaseOrder;
using Invoria.Procurement.Application.PurchaseOrders.Commands.ApprovePurchaseOrder;
using Invoria.Procurement.Contracts.PurchaseOrders;
using Invoria.Procurement.Domain.Parties;
using Invoria.Procurement.Domain.PurchaseOrders;
using Invoria.Procurement.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Procurement.Application.Tests.PurchaseOrders;

[TestFixture]
public class ApprovePurchaseOrderCommandHandlerTests : ProcurementTestFixture
{
    private IProcurementRepository<Supplier> SupplierRepository { get; }
    private IProcurementRepository<PurchaseOrder> PurchaseOrderRepository { get; }
    private IMediator Mediator { get; }

    public ApprovePurchaseOrderCommandHandlerTests()
    {
        SupplierRepository = ServiceProvider.GetRequiredService<IProcurementRepository<Supplier>>();
        PurchaseOrderRepository = ServiceProvider.GetRequiredService<IProcurementRepository<PurchaseOrder>>();
        Mediator = ServiceProvider.GetRequiredService<IMediator>();
    }

    [Test]
    public async Task Should_approve_purchase_order_when_it_is_submitted()
    {
        var created = await CreateSubmittedPurchaseOrderAsync();

        var command = new ApprovePurchaseOrderCommand(created.Id);
        var result = await Mediator.Send(command);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.State.Should().Be(PurchaseState.Approved);

        var purchaseOrder = await PurchaseOrderRepository.SingleOrDefault(x => x.Id == created.Id);
        purchaseOrder.Should().NotBeNull();
        purchaseOrder!.State.Should().Be(PurchaseState.Approved);
    }

    [Test]
    public async Task Should_fail_when_purchase_order_not_found()
    {
        var command = new ApprovePurchaseOrderCommand(Guid.NewGuid().ToString("N"));
        var result = await Mediator.Send(command);

        result.ShouldBeFailure(typeof(NotFoundException));
    }

    [Test]
    public async Task Should_fail_when_purchase_order_state_does_not_allow_approve()
    {
        var created = await CreateDraftPurchaseOrderAsync();

        var command = new ApprovePurchaseOrderCommand(created.Id);
        var result = await Mediator.Send(command);

        result.ShouldBeFailure(typeof(BusinessLogicException));
    }

    private async Task<PurchaseOrder> CreateSubmittedPurchaseOrderAsync()
    {
        var supplier = Supplier.Create(
            id: Guid.NewGuid().ToString("N"),
            supplierCode: "SUP-" + Guid.NewGuid().ToString("N")[..8],
            name: "Approve Supplier",
            contactEmail: "approve@example.com",
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

        // move to Submitted state using existing submit command
        var submitCommand = new SubmitPurchaseOrderCommand(purchaseOrder!.Id);
        var submitResult = await Mediator.Send(submitCommand);
        submitResult.ShouldBeSuccess();

        var submitted = await PurchaseOrderRepository.SingleOrDefault(x => x.Id == purchaseOrder.Id);
        submitted.Should().NotBeNull();
        submitted!.State.Should().Be(PurchaseState.Submitted);

        return submitted;
    }

    private async Task<PurchaseOrder> CreateDraftPurchaseOrderAsync()
    {
        var supplier = Supplier.Create(
            id: Guid.NewGuid().ToString("N"),
            supplierCode: "SUP-" + Guid.NewGuid().ToString("N")[..8],
            name: "Approve Supplier Draft",
            contactEmail: "approve-draft@example.com",
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
        purchaseOrder!.State.Should().Be(PurchaseState.Draft);

        return purchaseOrder;
    }
}

