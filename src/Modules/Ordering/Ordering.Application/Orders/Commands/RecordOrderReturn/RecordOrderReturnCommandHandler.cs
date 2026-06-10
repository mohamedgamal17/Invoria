using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Ordering.Application.Orders.Commands.RecordOrderReturn;

public sealed class RecordOrderReturnCommandHandler
    : IApplicatonRequestHandler<RecordOrderReturnCommand, Empty>
{
    private readonly IOrderingRepository<Order> _orderRepository;

    public RecordOrderReturnCommandHandler(IOrderingRepository<Order> orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<Empty>> Handle(
        RecordOrderReturnCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository
            .AsQuerable()
            .SingleAsync(o => o.Id == request.OrderId, cancellationToken);

        order.RecordReturn(request.ReturnId);

        await _orderRepository.Update(order, cancellationToken);

        return Result.Success(Empty.Value);
    }
}
