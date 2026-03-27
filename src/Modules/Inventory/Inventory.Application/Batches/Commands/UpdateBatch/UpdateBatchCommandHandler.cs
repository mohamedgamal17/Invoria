using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Application.Batches.Factories;
using Invoria.Inventory.Contracts.Dtos;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Application.Batches.Commands.UpdateBatch;

public class UpdateBatchCommandHandler : IApplicatonRequestHandler<UpdateBatchCommand, BatchDto>
{
    private readonly IInventoryRepository<Batch> _batchRepository;
    private readonly IBatchResponseFactory _batchResponseFactory;

    public UpdateBatchCommandHandler(
        IInventoryRepository<Batch> batchRepository,
        IBatchResponseFactory batchResponseFactory)
    {
        _batchRepository = batchRepository;
        _batchResponseFactory = batchResponseFactory;
    }

    public async Task<Result<BatchDto>> Handle(UpdateBatchCommand request, CancellationToken cancellationToken)
    {
        var batch = await _batchRepository.SingleOrDefault(x => x.Id == request.Id, cancellationToken);
        if (batch == null)
        {
            return Result.Failure<BatchDto>(new NotFoundException($"Batch with ID {request.Id} not found"));
        }

        batch.UpdateQuantity(request.Quantity);
        batch.UpdatePurchasePrice(request.PurchasePrice);
        await _batchRepository.Update(batch, cancellationToken);

        var dto = await _batchResponseFactory.PrepareDto(batch);
        return dto;
    }
}
