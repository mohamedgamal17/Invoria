using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Ordering.Application.Orders.Factories;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Ordering.Application.Orders.Queries.ListOrders;

public class ListQueryHandler : IApplicatonRequestHandler<ListOrdersQuery, PagingDto<OrderDto>>
{
    private readonly IOrderingRepository<Order> _orderRepository;
    private readonly IOrderResponseFactory _orderResponseFactory;

    public ListQueryHandler(
        IOrderingRepository<Order> orderRepository,
        IOrderResponseFactory orderResponseFactory)
    {
        _orderRepository = orderRepository;
        _orderResponseFactory = orderResponseFactory;
    }

    public async Task<Result<PagingDto<OrderDto>>> Handle(ListOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _orderRepository.AsQuerable();

        var orderNumberTerm = request.OrderNumber?.Trim();

        if (!string.IsNullOrEmpty(orderNumberTerm))
        {
            query = query.Where(o => o.OrderNumber.StartsWith(orderNumberTerm));
        }

        var customerIdTerm = request.CustomerId?.Trim();

        if (!string.IsNullOrEmpty(customerIdTerm))
        {
            query = query.Where(o => o.CustomerId == customerIdTerm);
        }

        if (request.PaymentType.HasValue)
        {
            query = query.Where(o => o.PaymentType == request.PaymentType.Value);
        }

        if (request.PaymentStatus.HasValue)
        {
            query = query.Where(o => o.PaymentStatus == request.PaymentStatus.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(o => o.Status == request.Status.Value);
        }

        query = query.OrderByDescending(o => o.Id);

        if (request.IncludeOrderItems || request.IncludeReturnItems)
        {
            query = query.Include(o => o.Items);
        }

        if (request.IncludeOrderItems)
        {
            query = query.Include(o => o.Payments);
        }

        if (request.IncludeReturnItems)
        {
            query = query.Include(o => o.ReturnItems);
        }

        var paged = await query.ToPaged(request.Skip, request.Length);

        var response = await _orderResponseFactory.PreparePagingDto(
            paged,
            request.IncludeOrderItems,
            request.IncludeReturnItems,
            cancellationToken);

        return response;
    }
}
