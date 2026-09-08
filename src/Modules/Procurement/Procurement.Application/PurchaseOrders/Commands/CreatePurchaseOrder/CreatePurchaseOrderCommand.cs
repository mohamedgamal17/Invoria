using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Procurement.Contracts.Dtos;

namespace Invoria.Procurement.Application.PurchaseOrders.Commands.CreatePurchaseOrder;

public sealed class CreatePurchaseOrderCommand : ICommand<PurchaseOrderDto>
{
    public string SupplierId { get; set; }
    public List<CreatePurchaseOrderItemCommand> PurchaseOrderItems { get; set; }

    public CreatePurchaseOrderCommand(
        string supplierId,
        List<CreatePurchaseOrderItemCommand> purchaseOrderItems)
    {
        SupplierId = supplierId;
        PurchaseOrderItems = purchaseOrderItems;
    }
}
