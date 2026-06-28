using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.Inventory.Contracts.Returns.Dtos;
using Invoria.Inventory.Domain.Returns;

namespace Invoria.Inventory.Application.Returns.Queries.ListReturns;

public class ListReturnsQuery : PagingParams, IQuery<PagingDto<ReturnDto>>
{
    public ReturnType? Type { get; set; }
}
