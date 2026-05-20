using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Procurement.Application.PurchaseOrders.Commands.CompletePurchaseOrder;
using Invoria.Procurement.Application.PurchaseOrders.Commands.CreatePurchaseOrder;
using Invoria.Procurement.Application.PurchaseOrders.Commands.SubmitPurchaseOrder;
using Invoria.Procurement.Application.PurchaseOrders.Commands.ApprovePurchaseOrder;
using Invoria.Procurement.Contracts.PurchaseOrders;
using Invoria.Procurement.Domain.Parties;
using Invoria.Procurement.Domain.PurchaseOrders;
using Invoria.Procurement.Domain.Repositories;
using Invoria.Procurement.Infrastructure.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Rebus.Bus;

namespace Invoria.Procurement.Application.Tests.PurchaseOrders;

[TestFixture]
public class CompletePurchaseOrderCommandHandlerTests : ProcurementTestFixture
{
    private IProcurementRepository<Supplier> SupplierRepository { get; }
    private IProcurementRepository<PurchaseOrder> PurchaseOrderRepository { get; }
    private IMediator Mediator { get; }

    public CompletePurchaseOrderCommandHandlerTests()
    {
        SupplierRepository = ServiceProvider.GetRequiredService<IProcurementRepository<Supplier>>();
        PurchaseOrderRepository = ServiceProvider.GetRequiredService<IProcurementRepository<PurchaseOrder>>();
        Mediator = ServiceProvider.GetRequiredService<IMediator>();
    }

    protected override async Task BeforeAnyTestRunAsync()
    {
        await ClearPurchaseOrdersAndSuppliersAsync();

        var busMock = ServiceProvider.GetRequiredService<Mock<IBus>>();
        busMock.Invocations.Clear();
    }

    [Test]
    public async Task Should_complete_purchase_order_when_it_is_approved_and_publish_integration_event()
    {
        var approved = await CreateApprovedPurchaseOrderAsync();

        var command = new CompletePurchaseOrderCommand(approved.Id);
        var result = await Mediator.Send(command);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.State.Should().Be(PurchaseState.Completed);

        var purchaseOrder = await PurchaseOrderRepository.SingleOrDefault(x => x.Id == approved.Id);
        purchaseOrder.Should().NotBeNull();
        purchaseOrder!.State.Should().Be(PurchaseState.Completed);

        var busMock = ServiceProvider.GetRequiredService<Mock<IBus>>();
        busMock.Verify(
            b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Should_fail_when_purchase_order_not_found()
    {
        var command = new CompletePurchaseOrderCommand(Guid.NewGuid().ToString("N"));
        var result = await Mediator.Send(command);

        result.ShouldBeFailure(typeof(NotFoundException));
    }

    [Test]
    public async Task Should_fail_when_purchase_order_state_does_not_allow_complete()
    {
        var created = await CreateSubmittedPurchaseOrderAsync();

        var command = new CompletePurchaseOrderCommand(created.Id);
        var result = await Mediator.Send(command);

        result.ShouldBeFailure(typeof(BusinessLogicException));

        var busMock = ServiceProvider.GetRequiredService<Mock<IBus>>();
        busMock.Verify(
            b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()),
            Times.Never);
    }

    private async Task<PurchaseOrder> CreateApprovedPurchaseOrderAsync()
    {
        var submitted = await CreateSubmittedPurchaseOrderAsync();
        var approveCommand = new ApprovePurchaseOrderCommand(submitted.Id);
        var approveResult = await Mediator.Send(approveCommand);
        approveResult.ShouldBeSuccess();

        var approved = await PurchaseOrderRepository.SingleOrDefault(x => x.Id == submitted.Id);
        approved.Should().NotBeNull();
        approved!.State.Should().Be(PurchaseState.Approved);
        return approved;
    }

    private async Task<PurchaseOrder> CreateSubmittedPurchaseOrderAsync()
    {
        var supplier = Supplier.Create(
            id: Guid.NewGuid().ToString("N"),
            supplierCode: "SUP-" + Guid.NewGuid().ToString("N")[..8],
            name: "Complete Supplier",
            contactEmail: "complete@example.com",
            phone: "+1",
            createdBy: null);
        await SupplierRepository.Add(supplier);

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

        var submitCommand = new SubmitPurchaseOrderCommand(purchaseOrder!.Id);
        var submitResult = await Mediator.Send(submitCommand);
        submitResult.ShouldBeSuccess();

        var submitted = await PurchaseOrderRepository.SingleOrDefault(x => x.Id == purchaseOrder.Id);
        submitted.Should().NotBeNull();
        submitted!.State.Should().Be(PurchaseState.Submitted);
        return submitted;
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

