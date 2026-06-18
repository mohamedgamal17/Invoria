using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Returns;

namespace Invoria.Inventory.Application.Returns.Commands.CreateImmediateReturn;

public class CreateImmediateReturnCommandHandler
    : IApplicatonRequestHandler<CreateImmediateReturnCommand, Empty>
{
    private readonly IInventoryRepository<ImmediateReturn> _immediateReturnRepository;

    public CreateImmediateReturnCommandHandler(
        IInventoryRepository<ImmediateReturn> immediateReturnRepository)
    {
        _immediateReturnRepository = immediateReturnRepository;
    }

    public async Task<Result<Empty>> Handle(
        CreateImmediateReturnCommand request,
        CancellationToken cancellationToken)
    {
        var returnLines = request.Lines
            .Select(l => ReturnLine.Create(l.OrderItemId, l.ProductId, l.Quantity))
            .ToList();

        var immediateReturn = ImmediateReturn.Create(
            request.AllocationId,
            request.OrderId,
            returnLines);

        await _immediateReturnRepository.Add(immediateReturn, cancellationToken);

        return Result.Success(Empty.Value);
    }
}
