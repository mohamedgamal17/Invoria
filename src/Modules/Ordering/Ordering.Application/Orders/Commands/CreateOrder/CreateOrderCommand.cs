using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Ordering.Contracts.Orders.Dtos;
using Invoria.Ordering.Contracts.Orders.Enums;

namespace Invoria.Ordering.Application.Orders.Commands.CreateOrder
{
    public class CreateOrderCommand : ICommand<OrderDto>
    {
        public string CustomerId { get; set; }
        public List<CreateOrderItemCommand> Items { get; set; }
        public OrderPaymentType PaymentType { get; set; }

        public CreateOrderCommand(
            string customerId,
            List<CreateOrderItemCommand> items,
            OrderPaymentType paymentType = OrderPaymentType.Immediate)
        {
            CustomerId = customerId;
            Items = items;
            PaymentType = paymentType;
        }
    }
}
