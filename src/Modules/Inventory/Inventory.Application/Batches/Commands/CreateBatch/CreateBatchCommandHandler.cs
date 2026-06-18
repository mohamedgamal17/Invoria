using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Application.Batches.Factories;
using Invoria.Inventory.Contracts.Batches.Dtos;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Application.Batches.Commands.CreateBatch;

public class CreateBatchCommandHandler : IApplicatonRequestHandler<CreateBatchCommand, BatchDto>
{
    private readonly IInventoryRepository<Batch> _batchRepository;
    private readonly IBatchResponseFactory _batchResponseFactory;

    public CreateBatchCommandHandler(
        IInventoryRepository<Batch> batchRepository,
        IBatchResponseFactory batchResponseFactory)
    {
        _batchRepository = batchRepository;
        _batchResponseFactory = batchResponseFactory;
    }

    public async Task<Result<BatchDto>> Handle(CreateBatchCommand request, CancellationToken cancellationToken)
    {
        var batch = new Batch(request.ProductId, request.Quantity,  request.PurchasePrice);

        await _batchRepository.Add(batch, cancellationToken);

        var dto = await _batchResponseFactory.PrepareDto(batch);

        return dto;
    }
}
