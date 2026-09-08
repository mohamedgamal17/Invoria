using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Procurement.Contracts.Dtos;

namespace Invoria.Procurement.Application.PurchaseOrders.Commands.UpdatePurchaseOrder;

public sealed class UpdatePurchaseOrderCommand : ICommand<PurchaseOrderDto>
{
    public string Id { get; set; }
    public string SupplierId { get; set; }
    public List<UpdatePurchaseOrderItemCommand> PurchaseOrderItems { get; set; }

    public UpdatePurchaseOrderCommand(
        string id,
        string supplierId,
        List<UpdatePurchaseOrderItemCommand> purchaseOrderItems)
    {
        Id = id;
        SupplierId = supplierId;
        PurchaseOrderItems = purchaseOrderItems;
    }
}

