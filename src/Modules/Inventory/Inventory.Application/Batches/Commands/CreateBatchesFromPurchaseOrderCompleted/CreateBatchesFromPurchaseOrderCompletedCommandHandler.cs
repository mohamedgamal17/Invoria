using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Application.Batches.Commands.CreateBatchesFromPurchaseOrderCompleted;

public sealed class CreateBatchesFromPurchaseOrderCompletedCommandHandler
    : IApplicatonRequestHandler<CreateBatchesFromPurchaseOrderCompletedCommand, Empty>
{
    private readonly IInventoryRepository<Batch> _batchRepository;

    public CreateBatchesFromPurchaseOrderCompletedCommandHandler(IInventoryRepository<Batch> batchRepository)
    {
        _batchRepository = batchRepository;
    }

    public async Task<Result<Empty>> Handle(
        CreateBatchesFromPurchaseOrderCompletedCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            foreach (var item in request.Items)
            {
                var batch = new Batch(
                    item.ProductId,
                    item.Quantity,
                    item.UnitPrice,
                    item.PurchaseOrderItemId);

                await _batchRepository.Add(batch, cancellationToken);
            }

            return Result.Success(Empty.Value);
        }
        catch (Exception ex)
        {
            return Result.Failure<Empty>(new InvalidOperationException($"Batch creation failed: {ex.Message}", ex));
        }
    }
}
