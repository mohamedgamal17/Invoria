using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Application.Orders.Factories;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Contracts.Events;
using Invoria.Ordering.Contracts.Models;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Orders.Commands.AcceptOrder;

public class AcceptOrderCommandHandler : IApplicatonRequestHandler<AcceptOrderCommand, OrderDto>
{
    private readonly IOrderingRepository<Order> _orderRepository;
    private readonly IOrderResponseFactory _orderResponseFactory;
    private readonly IBus _bus;

    public AcceptOrderCommandHandler(
        IOrderingRepository<Order> orderRepository,
        IOrderResponseFactory orderResponseFactory,
        IBus bus)
    {
        _orderRepository = orderRepository;
        _orderResponseFactory = orderResponseFactory;
        _bus = bus;
    }

    public async Task<Result<OrderDto>> Handle(AcceptOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository
            .AsQuerable()
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .SingleOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (order == null)
        {
            return Result.Failure<OrderDto>(new NotFoundException($"Order with ID {request.Id} not found"));
        }

        try
        {
            order.Accept();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<OrderDto>(new BusinessLogicException(ex.Message, ex));
        }

        await _orderRepository.Update(order, cancellationToken);

        await _bus.Publish(new AllocateOrderIntegrationEvent
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            Items = order.Items
                .Select(i => new OrderItemModel
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                })
                .ToList()
        });

        var dto = await _orderResponseFactory.PrepareDto(order);

        return dto;
    }
}
