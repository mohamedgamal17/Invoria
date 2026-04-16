using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Procurement.Contracts.Dtos;

namespace Invoria.Procurement.Application.PurchaseOrders.Commands.ReopenPurchaseOrder;

public sealed class ReopenPurchaseOrderCommand : ICommand<PurchaseOrderDto>
{
    public string Id { get; set; }

    public ReopenPurchaseOrderCommand(string id)
    {
        Id = id;
    }
}
