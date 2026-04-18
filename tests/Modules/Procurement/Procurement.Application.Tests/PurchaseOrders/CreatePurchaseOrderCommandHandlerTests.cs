using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.Procurement.Application.PurchaseOrders.Commands.CreatePurchaseOrder;
using Invoria.Procurement.Domain.Parties;
using Invoria.Procurement.Domain.PurchaseOrders;
using Invoria.Procurement.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Procurement.Application.Tests.PurchaseOrders;

[TestFixture]
public class CreatePurchaseOrderCommandHandlerTests : ProcurementTestFixture
{
    private IProcurementRepository<Supplier> SupplierRepository { get; }
    private IProcurementRepository<PurchaseOrder> PurchaseOrderRepository { get; }
    private IMediator Mediator { get; }

    public CreatePurchaseOrderCommandHandlerTests()
    {
        SupplierRepository = ServiceProvider.GetRequiredService<IProcurementRepository<Supplier>>();
        PurchaseOrderRepository = ServiceProvider.GetRequiredService<IProcurementRepository<PurchaseOrder>>();
        Mediator = ServiceProvider.GetRequiredService<IMediator>();
    }

    [Test]
    public async Task Should_create_purchase_order()
    {
        // Arrange
        var supplier = Supplier.Create(
            id: Guid.NewGuid().ToString("N"),
            supplierCode: "SUP-" + Guid.NewGuid().ToString("N")[..8],
            name: "Procurement Supplier",
            contactEmail: "supplier@example.com",
            phone: "+1",
            createdBy: null);

        await SupplierRepository.Add(supplier);

        var command = new CreatePurchaseOrderCommand(
            supplierId: supplier.Id,
            taxAmount: 10m,
            discountAmount: 5m,
            orderDate: DateTime.UtcNow.Date,
            expectedDeliveryDate: DateTime.UtcNow.Date.AddDays(5),
            purchaseOrderItems:
            [
                new CreatePurchaseOrderItemCommand(
                    productId: Guid.NewGuid().ToString("N"),
                    quantity: 3,
                    unitPrice: 100m,
                    supplierProductCode: "SKU-01")
            ]);

        // Act
        var result = await Mediator.Send(command);

        // Assert
        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();

        var purchaseOrder = await PurchaseOrderRepository.SingleOrDefault(x => x.Id == result.Value!.Id);
        purchaseOrder.Should().NotBeNull();
        purchaseOrder!.PurchaseNumber.Should().NotBeNullOrWhiteSpace();
        purchaseOrder.SupplierId.Should().Be(supplier.Id);
        purchaseOrder.TaxAmount.Should().Be(10m);
        purchaseOrder.DiscountAmount.Should().Be(5m);
        purchaseOrder.SubTotal.Should().Be(300m);
        purchaseOrder.TotalAmount.Should().Be(305m);

        result.Value.PurchaseNumber.Should().Be(purchaseOrder.PurchaseNumber);
        result.Value.SupplierId.Should().Be(purchaseOrder.SupplierId);
        result.Value.Supplier.Should().NotBeNull();
        result.Value.Supplier!.Id.Should().Be(supplier.Id);
        result.Value.Supplier.Name.Should().Be(supplier.Name);
        result.Value.Supplier.SupplierCode.Should().Be(supplier.SupplierCode);
        result.Value.Supplier.CreatedAt.Should().Be(supplier.CreatedAt);
        result.Value.TaxAmount.Should().Be(purchaseOrder.TaxAmount);
        result.Value.DiscountAmount.Should().Be(purchaseOrder.DiscountAmount);
        result.Value.SubTotal.Should().Be(purchaseOrder.SubTotal);
        result.Value.TotalAmount.Should().Be(purchaseOrder.TotalAmount);
        result.Value.PurchaseOrderItems.Should().HaveCount(1);
    }
}
