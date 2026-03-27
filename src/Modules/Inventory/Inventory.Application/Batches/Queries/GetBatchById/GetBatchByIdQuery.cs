using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Inventory.Contracts.Dtos;

namespace Invoria.Inventory.Application.Batches.Queries.GetBatchById;

public class GetBatchByIdQuery : IQuery<BatchDto>
{
    public string Id { get; set; } = string.Empty;
}
