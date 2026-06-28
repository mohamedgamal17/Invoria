using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Inventory.Application.Returns.Factories;
using Invoria.Inventory.Contracts.Returns.Dtos;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Returns;

namespace Invoria.Inventory.Application.Returns.Queries.ListReturns;

public class ListReturnsQueryHandler
    : IApplicatonRequestHandler<ListReturnsQuery, PagingDto<ReturnDto>>
{
    private readonly IInventoryRepository<Return> _returnRepository;
    private readonly IReturnResponseFactory _returnResponseFactory;

    public ListReturnsQueryHandler(
        IInventoryRepository<Return> returnRepository,
        IReturnResponseFactory returnResponseFactory)
    {
        _returnRepository = returnRepository;
        _returnResponseFactory = returnResponseFactory;
    }

    public async Task<Result<PagingDto<ReturnDto>>> Handle(
        ListReturnsQuery request, CancellationToken cancellationToken)
    {
        var query = _returnRepository.AsQuerable();

        if (request.Type.HasValue)
        {
            query = query.Where(x => x.Type == request.Type.Value);
        }

        query = query.OrderByDescending(x => x.Id);

        var paged = await query.ToPaged(request.Skip, request.Length);
        var response = await _returnResponseFactory.PreparePagingDto(paged);

        return response;
    }
}
