using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Ordering.Contracts.Dtos;

namespace Invoria.Ordering.Application.Orders.Commands.DispatchOrder;

public class DispatchOrderCommand : ICommand<OrderDto>
{
    public string Id { get; set; } = string.Empty;

    public DispatchOrderCommand(string id)
    {
        Id = id;
    }
}
