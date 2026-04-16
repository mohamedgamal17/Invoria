using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Domain.PurchaseOrders;

namespace Invoria.Procurement.Application.PurchaseOrders.Factories;

public sealed class PurchaseOrderResponseFactory : ResponseFactory<PurchaseOrder, PurchaseOrderDto>, IPurchaseOrderResponseFactory
{
    public override Task<PurchaseOrderDto> PrepareDto(PurchaseOrder view)
    {
        var dto = new PurchaseOrderDto
        {
            Id = view.Id,
            PurchaseNumber = view.PurchaseNumber,
            SupplierId = view.SupplierId,
            State = view.State,
            OrderDate = view.OrderDate,
            ExpectedDeliveryDate = view.ExpectedDeliveryDate,
            CompletedDate = view.CompletedDate,
            SubTotal = view.SubTotal,
            TaxAmount = view.TaxAmount,
            DiscountAmount = view.DiscountAmount,
            TotalAmount = view.TotalAmount,
            PurchaseOrderItems = view.Items
                .Select(x => new PurchaseOrderItemDto
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    SupplierProductCode = x.SupplierProductCode,
                    LineTotal = x.LineTotal
                })
                .ToList()
        };

        MapAudited(view, dto);

        return Task.FromResult(dto);
    }
}
