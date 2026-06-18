using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Application.Orders.Factories;
using Invoria.Ordering.Application.Orders.Services;
using Invoria.Ordering.Contracts.Orders.Dtos;
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
            var orderNumber = await _orderNumberGenerator.GenerateAsync(cancellationToken);

            var items = request.Items
                .Select(c => new OrderItem(c.ProductId, c.Quantity, c.Price))
                .ToList();

            var order = Order.Create(orderNumber, request.CustomerId, request.PaymentType, items);

            await _orderRepository.Add(order, cancellationToken);

            var dto = await _orderResponseFactory.PrepareDto(order);

            return dto;
        }
    }
}
