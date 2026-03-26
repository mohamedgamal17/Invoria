using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Inventory.Contracts.Dtos;
using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Application.Batches.Factories;

public interface IBatchResponseFactory : IResponseFactory<Batch, BatchDto>
{
}
