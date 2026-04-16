using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Procurement.Contracts.Dtos;

namespace Invoria.Procurement.Application.PurchaseOrders.Commands.SubmitPurchaseOrder;

public sealed class SubmitPurchaseOrderCommand : ICommand<PurchaseOrderDto>
{
    public string Id { get; set; }

    public SubmitPurchaseOrderCommand(string id)
    {
        Id = id;
    }
}
