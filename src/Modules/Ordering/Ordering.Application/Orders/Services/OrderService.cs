using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Application.Orders.Factories;
using Invoria.Ordering.Contracts.Orders.Dtos;
using Invoria.Ordering.Contracts.Orders.Services;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Invoria.Ordering.Application.Orders.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderingRepository<Order> _orderRepository;
        private readonly IOrderResponseFactory _orderResponseFactory;

        public OrderService(
            IOrderingRepository<Order> orderRepository,
            IOrderResponseFactory orderResponseFactory)
        {
            _orderRepository = orderRepository;
            _orderResponseFactory = orderResponseFactory;
        }

        public async Task<Result<PublicOrderDto>> GetOrderByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var order = await _orderRepository
                .AsQuerable()
                .Include(o => o.Items)
                .Include(o => o.ReturnItems)
                .SingleOrDefaultAsync(o => o.Id == id, cancellationToken);
            if (order == null)
            {
                return Result.Failure<PublicOrderDto>(new NotFoundException($"Order with ID {id} not found"));
            }

            var publicOrderDto = await _orderResponseFactory.PreparePublicOrderDto(order, cancellationToken);

            return Result.Success(publicOrderDto);
        }

        public async Task<Result<IReadOnlyList<PublicOrderDto>>> ListOrdersByIdsAsync(
            IEnumerable<string> ids,
            CancellationToken cancellationToken = default)
        {
            var idList = ids
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            if (idList.Count == 0)
            {
                return new List<PublicOrderDto>();
            }

            var orders = await _orderRepository
                .AsQuerable()
                .Include(o => o.Items)
                .Include(o => o.ReturnItems)
                .Where(o => idList.Contains(o.Id))
                .ToListAsync(cancellationToken);

            var publicOrderDtos = await _orderResponseFactory.PreparePublicListDto(orders, cancellationToken);

            return publicOrderDtos;
        }
    }
}