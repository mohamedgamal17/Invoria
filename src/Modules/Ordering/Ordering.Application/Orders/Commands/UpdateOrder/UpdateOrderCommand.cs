using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Ordering.Application.Orders.Commands.CreateOrder;
using Invoria.Ordering.Contracts.Orders.Dtos;

namespace Invoria.Ordering.Application.Orders.Commands.UpdateOrder;

public class UpdateOrderCommand : ICommand<OrderDto>
{
    public string Id { get; set; }
    public List<CreateOrderItemCommand> Items { get; set; }

    public UpdateOrderCommand(string id, List<CreateOrderItemCommand> items)
    {
        Id = id;
        Items = items;
    }
}
