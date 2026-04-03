using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Ordering.Application.Orders.Commands.RecordOrderAllocationSucceeded;

public sealed class RecordOrderAllocationSucceededCommandHandler
    : IApplicatonRequestHandler<RecordOrderAllocationSucceededCommand, Empty>
{
    private readonly IOrderingRepository<Order> _orderRepository;

    public RecordOrderAllocationSucceededCommandHandler(IOrderingRepository<Order> orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<Empty>> Handle(
        RecordOrderAllocationSucceededCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository
            .AsQuerable()
            .SingleOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            return Result.Failure<Empty>(new NotFoundException($"Order with ID {request.OrderId} not found"));
        }

        if (order.CustomerId != request.CustomerId)
        {
            return Result.Failure<Empty>(
                new BusinessLogicException(
                    "Order customer does not match the allocation success event."));
        }

        try
        {
            order.MarkInventoryAllocated();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<Empty>(new BusinessLogicException(ex.Message, ex));
        }

        await _orderRepository.Update(order, cancellationToken);

        return Result.Success(Empty.Value);
    }
}
