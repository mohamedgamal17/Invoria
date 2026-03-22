using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Application.Orders.Factories;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Ordering.Application.Orders.Queries.GetOrderById;

public class GetOrderByIdQueryHandler : IApplicatonRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IOrderingRepository<Order> _orderRepository;
    private readonly IOrderResponseFactory _orderResponseFactory;

    public GetOrderByIdQueryHandler(
        IOrderingRepository<Order> orderRepository,
        IOrderResponseFactory orderResponseFactory)
    {
        _orderRepository = orderRepository;
        _orderResponseFactory = orderResponseFactory;
    }

    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository
            .AsQuerable()
            .Include(o => o.Items)
            .SingleOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (order == null)
        {
            return Result.Failure<OrderDto>(new NotFoundException($"Order with ID {request.Id} not found"));
        }

        var dto = await _orderResponseFactory.PrepareDto(order);

        return dto;
    }
}
