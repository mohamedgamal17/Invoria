using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Inventory.Application.Batches.Factories;
using Invoria.Inventory.Contracts.Batches.Dtos;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Application.Batches.Queries.ListBatches;

public class ListBatchesQueryHandler : IApplicatonRequestHandler<ListBatchesQuery, PagingDto<BatchDto>>
{
    private readonly IInventoryRepository<Batch> _batchRepository;
    private readonly IBatchResponseFactory _batchResponseFactory;

    public ListBatchesQueryHandler(
        IInventoryRepository<Batch> batchRepository,
        IBatchResponseFactory batchResponseFactory)
    {
        _batchRepository = batchRepository;
        _batchResponseFactory = batchResponseFactory;
    }

    public async Task<Result<PagingDto<BatchDto>>> Handle(ListBatchesQuery request, CancellationToken cancellationToken)
    {
        var query = _batchRepository.AsQuerable();

        var productId = request.ProductId?.Trim();
        if (!string.IsNullOrEmpty(productId))
        {
            query = query.Where(x => x.ProductId == productId);
        }

        if (request.State.HasValue)
        {
            query = query.Where(x => x.State == request.State.Value);
        }

        query = query.OrderByDescending(x => x.Id);

        var paged = await query.ToPaged(request.Skip, request.Length);
        var response = await _batchResponseFactory.PreparePagingDto(paged);

        return response;
    }
}
