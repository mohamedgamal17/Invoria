using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.Inventory.Contracts.Dtos;
using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Application.Batches.Queries.ListBatches;

public class ListBatchesQuery : PagingParams, IQuery<PagingDto<BatchDto>>
{
    public string? ProductId { get; set; }
    public BatchState? State { get; set; }
}
