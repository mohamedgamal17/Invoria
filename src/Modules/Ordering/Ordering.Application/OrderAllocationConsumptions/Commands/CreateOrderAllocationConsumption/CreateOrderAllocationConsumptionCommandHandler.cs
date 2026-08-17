using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Application.OrderAllocationConsumptions.Extensions;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.OrderAllocationConsumptions;

namespace Invoria.Ordering.Application.OrderAllocationConsumptions.Commands.CreateOrderAllocationConsumption;

public sealed class CreateOrderAllocationConsumptionCommandHandler
    : IApplicatonRequestHandler<CreateOrderAllocationConsumptionCommand, Empty>
{
    private readonly IOrderingRepository<OrderAllocationConsumption> _consumptionRepository;

    public CreateOrderAllocationConsumptionCommandHandler(
        IOrderingRepository<OrderAllocationConsumption> consumptionRepository)
    {
        _consumptionRepository = consumptionRepository;
    }

    public async Task<Result<Empty>> Handle(
        CreateOrderAllocationConsumptionCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await _consumptionRepository.SingleOrDefault(
            c => c.AllocationId == request.AllocationId,
            cancellationToken);

        if (existing is not null)
        {
            return Result.Success(Empty.Value);
        }

        var consumption = request.ToOrderAllocationConsumption();

        await _consumptionRepository.Add(consumption, cancellationToken);

        return Result.Success(Empty.Value);
    }
}
