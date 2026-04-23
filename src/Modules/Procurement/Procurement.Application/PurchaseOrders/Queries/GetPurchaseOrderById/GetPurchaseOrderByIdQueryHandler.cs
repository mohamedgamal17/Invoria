using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Procurement.Application.PurchaseOrders.Factories;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Domain.PurchaseOrders;
using Invoria.Procurement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Procurement.Application.PurchaseOrders.Queries.GetPurchaseOrderById;

public sealed class GetPurchaseOrderByIdQueryHandler : IApplicatonRequestHandler<GetPurchaseOrderByIdQuery, PurchaseOrderDto>
{
    private readonly IProcurementRepository<PurchaseOrder> _purchaseOrderRepository;
    private readonly IPurchaseOrderResponseFactory _purchaseOrderResponseFactory;

    public GetPurchaseOrderByIdQueryHandler(
        IProcurementRepository<PurchaseOrder> purchaseOrderRepository,
        IPurchaseOrderResponseFactory purchaseOrderResponseFactory)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _purchaseOrderResponseFactory = purchaseOrderResponseFactory;
    }

    public async Task<Result<PurchaseOrderDto>> Handle(GetPurchaseOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var purchaseOrder = await _purchaseOrderRepository
            .AsQuerable()
            .AsSplitQuery()
            .Include(x => x.Supplier)
            .Include(x => x.Items)
            .Include(x => x.StateHistory)
            .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (purchaseOrder == null)
        {
            return Result.Failure<PurchaseOrderDto>(
                new NotFoundException($"Purchase order with ID {request.Id} not found"));
        }

        var dto = await _purchaseOrderResponseFactory.PrepareDto(purchaseOrder);
        return dto;
    }
}
