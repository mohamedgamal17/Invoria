using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Ordering.Contracts.Dtos;

namespace Invoria.Ordering.Application.Orders.Commands.AcceptOrder;

public class AcceptOrderCommand : ICommand<OrderDto>
{
    public string Id { get; set; }

    public AcceptOrderCommand(string id)
    {
        Id = id;
    }
}
