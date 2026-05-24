using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Allocations;

namespace Invoria.Inventory.Application.Batches.Commands.AllocateOrder;

public sealed class AllocateOrderCommandHandler
    : IApplicatonRequestHandler<AllocateOrderCommand, Empty>
{
    private readonly IInventoryRepository<Allocation> _allocationRepository;

    public AllocateOrderCommandHandler(IInventoryRepository<Allocation> allocationRepository)
    {
        _allocationRepository = allocationRepository;
    }

    public async Task<Result<Empty>> Handle(
        AllocateOrderCommand request,
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
