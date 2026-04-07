using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Ordering.Application.Orders.Commands.CompleteRefusalAfterInventoryReleased;

public sealed class CompleteRefusalAfterInventoryReleasedCommandHandler
    : IApplicatonRequestHandler<CompleteRefusalAfterInventoryReleasedCommand, Empty>
{
    private readonly IOrderingRepository<Order> _orderRepository;

    public CompleteRefusalAfterInventoryReleasedCommandHandler(IOrderingRepository<Order> orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<Empty>> Handle(
        CompleteRefusalAfterInventoryReleasedCommand request,
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

        if (order.CustomerId != request.CustomerId)
        {
            return Result.Failure<Empty>(
                new BusinessLogicException("Order customer does not match the inventory refusal release event."));
        }

        if (order.OrderNumber != request.OrderNumber)
        {
            return Result.Failure<Empty>(
                new BusinessLogicException("Order number does not match the inventory refusal release event."));
        }

        try
        {
            order.CompleteRefusalAfterInventoryReleased();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<Empty>(new BusinessLogicException(ex.Message, ex));
        }

        await _orderRepository.Update(order, cancellationToken);

        return Result.Success(Empty.Value);
    }
}
