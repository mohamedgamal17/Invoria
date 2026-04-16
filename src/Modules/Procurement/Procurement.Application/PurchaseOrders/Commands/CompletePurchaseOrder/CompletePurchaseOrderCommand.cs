using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Procurement.Contracts.Dtos;

namespace Invoria.Procurement.Application.PurchaseOrders.Commands.CompletePurchaseOrder;

public sealed class CompletePurchaseOrderCommand : ICommand<PurchaseOrderDto>
{
    public string Id { get; set; }

    public CompletePurchaseOrderCommand(string id)
    {
        Id = id;
    }
}

