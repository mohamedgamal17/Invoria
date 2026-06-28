using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Inventory.Application.Allocations.Dtos;
using Invoria.Inventory.Domain.Allocations;

namespace Invoria.Inventory.Application.Allocations.Factories;

public interface IAllocationResponseFactory : IResponseFactory<Allocation, AllocationDto>
{
}
