using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Domain.PurchaseOrders;

namespace Invoria.Procurement.Application.PurchaseOrders.Factories;

public interface IPurchaseOrderResponseFactory : IResponseFactory<PurchaseOrder, PurchaseOrderDto>
{
}
