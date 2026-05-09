using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Contracts.PurchaseOrders;

namespace Invoria.Procurement.Application.PurchaseOrders.Queries.ListPurchaseOrders;

public sealed class ListPurchaseOrdersQuery : PagingParams, IQuery<PagingDto<PurchaseOrderDto>>
{
    public string? Number { get; set; }

    /// <summary>
    /// When set, only purchase orders in this lifecycle state are returned.
    /// </summary>
    public PurchaseState? Status { get; set; }

    public bool IncludePurchaseItems { get; set; }

    public bool IncludeSupplier { get; set; }
}
