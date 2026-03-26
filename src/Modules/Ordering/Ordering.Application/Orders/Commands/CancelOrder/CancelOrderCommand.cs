using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Ordering.Contracts.Dtos;

namespace Invoria.Ordering.Application.Orders.Commands.CancelOrder;

public class CancelOrderCommand : ICommand<OrderDto>
{
    public string Id { get; set; } = string.Empty;

    public CancelOrderCommand(string id)
    {
        Id = id;
    }
}

