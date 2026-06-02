using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Ordering.Contracts.Orders.Dtos;

namespace Invoria.Ordering.Application.Orders.Commands.AddReturnItems;

public class AddReturnItemsCommand : ICommand<OrderDto>
{
    public string Id { get; set; }

    public List<AddReturnItemLine> Items { get; set; }

    public AddReturnItemsCommand(string id, List<AddReturnItemLine> items)
    {
        Id = id;
        Items = items;
    }
}

public class AddReturnItemLine
{
    public string OrderItemId { get; set; }

    public int Quantity { get; set; }

    public AddReturnItemLine(string orderItemId, int quantity)
    {
        OrderItemId = orderItemId;
        Quantity = quantity;
    }
}
