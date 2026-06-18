using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Ordering.Contracts.Orders.Dtos;

namespace Invoria.Ordering.Application.Orders.Commands.RequestOrderRevision;

public sealed class RequestOrderRevisionCommand : ICommand<OrderDto>
{
    public string Id { get; set; }

    public RequestOrderRevisionCommand(string id)
    {
        Id = id;
    }
}
