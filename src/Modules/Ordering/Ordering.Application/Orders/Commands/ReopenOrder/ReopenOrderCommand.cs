using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Ordering.Contracts.Dtos;

namespace Invoria.Ordering.Application.Orders.Commands.ReopenOrder;

public class ReopenOrderCommand : ICommand<OrderDto>
{
    public string Id { get; set; }

    public ReopenOrderCommand(string id)
    {
        Id = id;
    }
}
