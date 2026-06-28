using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Inventory.Contracts.Returns.Dtos;
using Invoria.Inventory.Domain.Returns;

namespace Invoria.Inventory.Application.Returns.Factories;

public interface IImmediateReturnResponseFactory : IResponseFactory<ImmediateReturn, ReturnDto>
{
    ReturnDto PrepareDto(ImmediateReturn view, Dictionary<string, ProductDto> products);
}
