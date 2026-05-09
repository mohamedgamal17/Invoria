using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Application.Orders.Commands.CreateOrder;
using Invoria.Ordering.Application.Orders.Factories;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Ordering.Application.Orders.Commands.UpdateOrder;

public class UpdateOrderCommandHandler : IApplicatonRequestHandler<UpdateOrderCommand, OrderDto>
{
    private readonly IOrderingRepository<Order> _orderRepository;
    private readonly IOrderResponseFactory _orderResponseFactory;

    public UpdateOrderCommandHandler(
        IOrderingRepository<Order> orderRepository,
        IOrderResponseFactory orderResponseFactory)
    {
        _orderRepository = orderRepository;
        _orderResponseFactory = orderResponseFactory;
    }

    public async Task<Result<OrderDto>> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var itemCommands = request.Items ?? new List<CreateOrderItemCommand>();

        if (itemCommands.Count == 0)
        {
            return new InvalidOperationException("Order items must have one or more item.");
        }

        var order = await _orderRepository
            .AsQuerable()
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .SingleOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (order == null)
        {
            return Result.Failure<OrderDto>(new NotFoundException($"Order with ID {request.Id} not found"));
        }

        var items = itemCommands
            .Select(c => new OrderItem(c.ProductId, c.Quantity, c.Price))
            .ToList();

        try
        {
            order.UpdateItems(items);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<OrderDto>(new BusinessLogicException(ex.Message, ex));
        }

        await _orderRepository.Update(order, cancellationToken);

        var dto = await _orderResponseFactory.PrepareDto(order);

        return dto;
    }
}
