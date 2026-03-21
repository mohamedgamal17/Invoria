using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Ordering.Contracts.Dtos;

namespace Invoria.Ordering.Application.Orders.Commands.CreateOrder
{
    public class CreateOrderCommand : ICommand<OrderDto>
    {
        public string CustomerId { get; set; }
        public List<CreateOrderItemCommand> Items { get; set; }

        public CreateOrderCommand(string customerId, List<CreateOrderItemCommand> items)
        {
            CustomerId = customerId;
            Items = items;
        }
    }
}
