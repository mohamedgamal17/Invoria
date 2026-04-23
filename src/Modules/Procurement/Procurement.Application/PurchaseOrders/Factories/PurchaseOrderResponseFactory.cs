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
            Supplier = view.Supplier == null
                ? null
                : new PurchaseOrderSupplierSummaryDto
                {
                    Id = view.Supplier.Id,
                    SupplierCode = view.Supplier.SupplierCode,
                    Name = view.Supplier.Name
                },
            State = view.State,
            OrderDate = view.OrderDate,
            ExpectedDeliveryDate = view.ExpectedDeliveryDate,
            CompletedDate = view.CompletedDate,
            SubTotal = view.SubTotal,
            TaxAmount = view.TaxAmount,
            DiscountAmount = view.DiscountAmount,
            TotalAmount = view.TotalAmount,
            StateHistory = view.StateHistory
                .OrderBy(x => x.ChangedAt)
                .Select(x => new PurchaseOrderStateHistoryDto
                {
                    FromState = x.FromState,
                    ToState = x.ToState,
                    ChangedAt = x.ChangedAt,
                    Reason = x.Reason
                })
                .ToList(),
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
        if (view.Supplier != null && dto.Supplier != null)
        {
            MapAudited(view.Supplier, dto.Supplier);
        }

        return Task.FromResult(dto);
    }
}
