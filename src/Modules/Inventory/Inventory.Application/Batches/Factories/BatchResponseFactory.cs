using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Inventory.Contracts.Batches.Dtos;
using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Application.Batches.Factories;

public class BatchResponseFactory : ResponseFactory<Batch, BatchDto>, IBatchResponseFactory
{
    public override Task<BatchDto> PrepareDto(Batch view)
    {
        var dto = new BatchDto
        {
            Id = view.Id,
            ProductId = view.ProductId,
            PurchaseOrderItemId = view.PurchaseOrderItemId,
            Quantity = view.Quantity,
            ReservedQuantity = view.ReservedQuantity,
            PurchasePrice = view.PurchasePrice
        };

        MapAudited(view, dto);

        return Task.FromResult(dto);
    }
}
