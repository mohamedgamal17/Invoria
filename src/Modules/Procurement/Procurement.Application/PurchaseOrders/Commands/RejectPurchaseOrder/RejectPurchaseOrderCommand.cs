using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Procurement.Contracts.Dtos;

namespace Invoria.Procurement.Application.PurchaseOrders.Commands.RejectPurchaseOrder;

public sealed class RejectPurchaseOrderCommand : ICommand<PurchaseOrderDto>
{
    public string Id { get; set; }

    public RejectPurchaseOrderCommand(string id)
    {
        Id = id;
    }
}
