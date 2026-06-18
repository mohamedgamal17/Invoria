using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Application.Batches.Factories;
using Invoria.Inventory.Contracts.Batches.Dtos;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Application.Batches.Queries.GetBatchById;

public class GetBatchByIdQueryHandler : IApplicatonRequestHandler<GetBatchByIdQuery, BatchDto>
{
    private readonly IInventoryRepository<Batch> _batchRepository;
    private readonly IBatchResponseFactory _batchResponseFactory;

    public GetBatchByIdQueryHandler(
        IInventoryRepository<Batch> batchRepository,
        IBatchResponseFactory batchResponseFactory)
    {
        _batchRepository = batchRepository;
        _batchResponseFactory = batchResponseFactory;
    }

    public async Task<Result<BatchDto>> Handle(GetBatchByIdQuery request, CancellationToken cancellationToken)
    {
        var batch = await _batchRepository.SingleOrDefault(x => x.Id == request.Id, cancellationToken);
        if (batch == null)
        {
            return Result.Failure<BatchDto>(new NotFoundException($"Batch with ID {request.Id} not found"));
        }

        var dto = await _batchResponseFactory.PrepareDto(batch);
        return dto;
    }
}
