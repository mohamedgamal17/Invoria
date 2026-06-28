using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Inventory.Contracts.Returns.Dtos;

namespace Invoria.Inventory.Application.Returns.Queries.GetReturnById;

public class GetReturnByIdQuery : IQuery<ReturnDto>
{
    public string Id { get; set; } = string.Empty;
}
