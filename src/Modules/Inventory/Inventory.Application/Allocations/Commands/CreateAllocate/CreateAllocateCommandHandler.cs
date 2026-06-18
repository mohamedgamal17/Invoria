using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Allocations;

namespace Invoria.Inventory.Application.Allocations.Commands.CreateAllocate;

public sealed class CreateAllocateCommandHandler
    : IApplicatonRequestHandler<CreateAllocateCommand, Empty>
{
    private readonly IInventoryRepository<Allocation> _allocationRepository;

    public CreateAllocateCommandHandler(IInventoryRepository<Allocation> allocationRepository)
    {
        _allocationRepository = allocationRepository;
    }

    public async Task<Result<Empty>> Handle(
        CreateAllocateCommand request,
        CancellationToken cancellationToken)
    {
        var lines = request.Items
            .Select(i => (i.Id, i.ProductId, i.Quantity))
            .ToList();

        var allocation = Allocation.CreateForOrder(request.Id, lines);
        await _allocationRepository.Add(allocation, cancellationToken);

        return Result.Success(Empty.Value);
    }
}
