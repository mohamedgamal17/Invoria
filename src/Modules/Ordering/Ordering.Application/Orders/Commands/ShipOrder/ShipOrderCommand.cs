using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Ordering.Contracts.Dtos;

namespace Invoria.Ordering.Application.Orders.Commands.ShipOrder;

public class ShipOrderCommand : ICommand<OrderDto>
{
    public string Id { get; set; } = string.Empty;

    public ShipOrderCommand(string id)
    {
        Id = id;
    }
}
