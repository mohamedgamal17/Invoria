using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Procurement.Application.PurchaseOrders.Factories;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Domain.PurchaseOrders;
using Invoria.Procurement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Procurement.Application.PurchaseOrders.Commands.UpdatePurchaseOrder;

public sealed class UpdatePurchaseOrderCommandHandler : IApplicatonRequestHandler<UpdatePurchaseOrderCommand, PurchaseOrderDto>
{
    private readonly IProcurementRepository<PurchaseOrder> _purchaseOrderRepository;
    private readonly IPurchaseOrderResponseFactory _purchaseOrderResponseFactory;

    public UpdatePurchaseOrderCommandHandler(
        IProcurementRepository<PurchaseOrder> purchaseOrderRepository,
        IPurchaseOrderResponseFactory purchaseOrderResponseFactory)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _purchaseOrderResponseFactory = purchaseOrderResponseFactory;
    }

    public async Task<Result<PurchaseOrderDto>> Handle(UpdatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var purchaseOrder = await _purchaseOrderRepository
            .AsQuerable()
            .Include(x => x.Supplier)
            .Include(x => x.Items)
            .Include(x => x.StateHistory)
            .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (purchaseOrder == null)
        {
            return Result.Failure<PurchaseOrderDto>(new NotFoundException($"Purchase order with ID {request.Id} not found"));
        }

        try
        {
            purchaseOrder.UpdateHeader(
                supplierId: request.SupplierId,
                orderDate: request.OrderDate,
                expectedDeliveryDate: request.ExpectedDeliveryDate,
                taxAmount: request.TaxAmount,
                discountAmount: request.DiscountAmount);

            var lineCommands = request.PurchaseOrderItems ?? [];
            var newItems = lineCommands
                .Select(x => new PurchaseOrderItem(
                    id: Guid.NewGuid().ToString("N"),
                    purchaseOrderId: purchaseOrder.Id,
                    productId: x.ProductId,
                    quantity: x.Quantity,
                    unitPrice: x.UnitPrice,
                    supplierProductCode: x.SupplierProductCode))
                .ToList();

            purchaseOrder.ReplaceItems(newItems);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<PurchaseOrderDto>(new BusinessLogicException(ex.Message, ex));
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<PurchaseOrderDto>(new BusinessLogicException(ex.Message, ex));
        }

        await _purchaseOrderRepository.Update(purchaseOrder, cancellationToken);

        var dto = await _purchaseOrderResponseFactory.PrepareDto(purchaseOrder);
        return dto;
    }
}

