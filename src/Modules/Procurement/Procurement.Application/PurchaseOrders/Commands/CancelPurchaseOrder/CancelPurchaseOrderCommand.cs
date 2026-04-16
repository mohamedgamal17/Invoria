using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Procurement.Contracts.Dtos;

namespace Invoria.Procurement.Application.PurchaseOrders.Commands.CancelPurchaseOrder;

public sealed class CancelPurchaseOrderCommand : ICommand<PurchaseOrderDto>
{
    public string Id { get; set; }

    public CancelPurchaseOrderCommand(string id)
    {
        Id = id;
    }
}
