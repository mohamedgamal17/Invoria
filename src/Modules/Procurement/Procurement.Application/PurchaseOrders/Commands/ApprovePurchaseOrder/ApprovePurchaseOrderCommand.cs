using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Procurement.Contracts.Dtos;

namespace Invoria.Procurement.Application.PurchaseOrders.Commands.ApprovePurchaseOrder;

public sealed class ApprovePurchaseOrderCommand : ICommand<PurchaseOrderDto>
{
    public string Id { get; set; }

    public ApprovePurchaseOrderCommand(string id)
    {
        Id = id;
    }
}

