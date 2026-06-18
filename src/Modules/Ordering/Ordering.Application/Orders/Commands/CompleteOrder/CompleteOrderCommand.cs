using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Ordering.Contracts.Orders.Dtos;

namespace Invoria.Ordering.Application.Orders.Commands.CompleteOrder;

public class CompleteOrderCommand : ICommand<OrderDto>
{
    public string Id { get; set; }

    public List<CompleteReturnItemLine> Items { get; set; }

    public CompleteOrderCommand(string id, List<CompleteReturnItemLine>? items = null)
    {
        Id = id;
        Items = items ?? [];
    }
}

public class CompleteReturnItemLine
{
    public string OrderItemId { get; set; }

    public int Quantity { get; set; }

    public CompleteReturnItemLine(string orderItemId, int quantity)
    {
        OrderItemId = orderItemId;
        Quantity = quantity;
    }
}
