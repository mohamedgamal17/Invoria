using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Ordering.Application.Orders.Commands.RecordOrderAllocation;

public sealed class RecordOrderAllocationCommandHandler
    : IApplicatonRequestHandler<RecordOrderAllocationCommand, Empty>
{
    private readonly IOrderingRepository<Order> _orderRepository;

    public RecordOrderAllocationCommandHandler(IOrderingRepository<Order> orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<Empty>> Handle(
        RecordOrderAllocationCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository
            .AsQuerable()
            .SingleOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            return Result.Failure<Empty>(
                new NotFoundException($"Order with ID {request.OrderId} not found"));
        }

        try
        {
            order.RecordAllocation(request.AllocationId);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<Empty>(new BusinessLogicException(ex.Message, ex));
        }

        await _orderRepository.Update(order, cancellationToken);

        return Result.Success(Empty.Value);
    }
}
