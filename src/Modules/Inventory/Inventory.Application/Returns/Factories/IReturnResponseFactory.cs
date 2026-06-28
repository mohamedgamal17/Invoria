using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Inventory.Contracts.Returns.Dtos;
using Invoria.Inventory.Domain.Returns;

namespace Invoria.Inventory.Application.Returns.Factories;

public interface IReturnResponseFactory : IResponseFactory<Return, ReturnDto>
{
}
