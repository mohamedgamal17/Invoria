using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Ordering.Application.Orders.Commands.ReviseOrder;

public sealed class ReviseOrderCommandHandler
    : IApplicatonRequestHandler<ReviseOrderCommand, Empty>
{
    private readonly IOrderingRepository<Order> _orderRepository;

    public ReviseOrderCommandHandler(IOrderingRepository<Order> orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<Empty>> Handle(
        ReviseOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository
            .AsQuerable()
            .SingleAsync(o => o.Id == request.OrderId, cancellationToken);

        order.Revise();

        await _orderRepository.Update(order, cancellationToken);

        return Result.Success(Empty.Value);
    }
}
