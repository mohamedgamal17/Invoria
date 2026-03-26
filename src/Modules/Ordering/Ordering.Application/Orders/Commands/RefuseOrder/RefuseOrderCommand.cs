using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Ordering.Contracts.Dtos;

namespace Invoria.Ordering.Application.Orders.Commands.RefuseOrder;

public class RefuseOrderCommand : ICommand<OrderDto>
{
    public string Id { get; set; } = string.Empty;

    public RefuseOrderCommand(string id)
    {
        Id = id;
    }
}

