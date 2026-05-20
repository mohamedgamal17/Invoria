using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Procurement.Application.PurchaseOrders.Factories;
using Invoria.Procurement.Application.Services;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Domain.PurchaseOrders;
using Invoria.Procurement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Procurement.Application.PurchaseOrders.Commands.CreatePurchaseOrder;

public sealed class CreatePurchaseOrderCommandHandler : IApplicatonRequestHandler<CreatePurchaseOrderCommand, PurchaseOrderDto>
{
    private readonly IProcurementRepository<PurchaseOrder> _purchaseOrderRepository;
    private readonly IPurchaseOrderNumberGenerator _purchaseOrderNumberGenerator;
    private readonly IPurchaseOrderResponseFactory _purchaseOrderResponseFactory;

    public CreatePurchaseOrderCommandHandler(
        IProcurementRepository<PurchaseOrder> purchaseOrderRepository,
        IPurchaseOrderNumberGenerator purchaseOrderNumberGenerator,
        IPurchaseOrderResponseFactory purchaseOrderResponseFactory)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _purchaseOrderNumberGenerator = purchaseOrderNumberGenerator;
        _purchaseOrderResponseFactory = purchaseOrderResponseFactory;
    }

    public async Task<Result<PurchaseOrderDto>> Handle(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var lineCommands = request.PurchaseOrderItems ?? [];

        if (lineCommands.Count == 0)
        {
            return new InvalidOperationException("Purchase order must have one or more item.");
        }

        var purchaseOrderNumber = await _purchaseOrderNumberGenerator.GenerateNextAsync(cancellationToken);
        var purchaseOrder = new PurchaseOrder(
            id: Guid.NewGuid().ToString("N"),
            purchaseNumber: purchaseOrderNumber,
            supplierId: request.SupplierId);

        purchaseOrder.SetHeaderFinancials(request.TaxAmount, request.DiscountAmount);

        foreach (var line in lineCommands)
        {
            var item = new PurchaseOrderItem(
                id: Guid.NewGuid().ToString("N"),
                purchaseOrderId: purchaseOrder.Id,
                productId: line.ProductId,
                quantity: line.Quantity,
                unitPrice: line.UnitPrice,
                supplierProductCode: line.SupplierProductCode);

            purchaseOrder.AddItem(item);
        }

        await _purchaseOrderRepository.Add(purchaseOrder, cancellationToken);

        var persistedPurchaseOrder = await _purchaseOrderRepository
            .AsQuerable()
            .Include(x => x.Supplier)
            .Include(x => x.Items)
            .SingleAsync(x => x.Id == purchaseOrder.Id, cancellationToken);

        var dto = await _purchaseOrderResponseFactory.PrepareDto(persistedPurchaseOrder);
        return dto;
    }
}
