using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Procurement.Application.PurchaseOrders.Factories;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Domain.PurchaseOrders;
using Invoria.Procurement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Procurement.Application.PurchaseOrders.Queries.ListPurchaseOrders;

public sealed class ListPurchaseOrdersQueryHandler : IApplicatonRequestHandler<ListPurchaseOrdersQuery, PagingDto<PurchaseOrderDto>>
{
    private readonly IProcurementRepository<PurchaseOrder> _purchaseOrderRepository;
    private readonly IPurchaseOrderResponseFactory _purchaseOrderResponseFactory;

    public ListPurchaseOrdersQueryHandler(
        IProcurementRepository<PurchaseOrder> purchaseOrderRepository,
        IPurchaseOrderResponseFactory purchaseOrderResponseFactory)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _purchaseOrderResponseFactory = purchaseOrderResponseFactory;
    }

    public async Task<Result<PagingDto<PurchaseOrderDto>>> Handle(
        ListPurchaseOrdersQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<PurchaseOrder> query = _purchaseOrderRepository
            .AsQuerable()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.StateHistory);

        if (request.IncludePurchaseItems)
        {
            query = query.Include(x => x.Items);
        }

        if (request.IncludeSupplier)
        {
            query = query.Include(x => x.Supplier);
        }

        var numberTerm = request.Number?.Trim();
        if (!string.IsNullOrEmpty(numberTerm))
        {
            query = query.Where(x => EF.Functions.Like(x.PurchaseNumber, $"%{numberTerm}%"));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.State == request.Status.Value);
        }

        query = query.OrderByDescending(x => x.Id);

        var paged = await query.ToPaged(request.Skip, request.Length);
        var response = await _purchaseOrderResponseFactory.PreparePagingDto(paged);

        return response;
    }
}
