using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Ordering.Contracts.Orders.Dtos;

namespace Invoria.Ordering.Application.Orders.Commands.CompleteOrder;

public class CompleteOrderCommand : ICommand<OrderDto>
{
    public string Id { get; set; }

    public CompleteOrderCommand(string id)
    {
        Id = id;
    }
}
