using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Application.Orders.Factories;
using Invoria.Ordering.Application.Orders.Services;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Orders.Commands.CreateOrder
{
    public class CreateOrderCommandHandler : IApplicatonRequestHandler<CreateOrderCommand, OrderDto>
    {
        private readonly IOrderingRepository<Order> _orderRepository;
        private readonly IOrderNumberGenerator _orderNumberGenerator;
        private readonly IOrderResponseFactory _orderResponseFactory;

        public CreateOrderCommandHandler(
            IOrderingRepository<Order> orderRepository,
            IOrderNumberGenerator orderNumberGenerator,
            IOrderResponseFactory orderResponseFactory)
        {
            _orderRepository = orderRepository;
            _orderNumberGenerator = orderNumberGenerator;
            _orderResponseFactory = orderResponseFactory;
        }

        public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var itemCommands = request.Items ?? new List<CreateOrderItemCommand>();

            if (itemCommands.Count == 0)
            {
                return new InvalidOperationException("Order items must have one or more item.");
            }

            var orderNumber = await _orderNumberGenerator.GenerateAsync(cancellationToken);

            var order = new Order(orderNumber, request.CustomerId, request.PaymentType);

            var items = itemCommands
                .Select(c => new OrderItem(c.ProductId, c.Quantity, c.Price))
                .ToList();

            order.UpdateItems(items);

            await _orderRepository.Add(order, cancellationToken);

            var dto = await _orderResponseFactory.PrepareDto(order);

            return dto;
        }
    }
}
