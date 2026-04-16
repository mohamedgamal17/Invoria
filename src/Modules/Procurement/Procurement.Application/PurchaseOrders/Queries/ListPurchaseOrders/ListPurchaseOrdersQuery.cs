using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.Procurement.Contracts.Dtos;

namespace Invoria.Procurement.Application.PurchaseOrders.Queries.ListPurchaseOrders;

public sealed class ListPurchaseOrdersQuery : PagingParams, IQuery<PagingDto<PurchaseOrderDto>>
{
    public string? Number { get; set; }

    public bool IncludePurchaseItems { get; set; }

    public bool IncludeSupplier { get; set; }
}
