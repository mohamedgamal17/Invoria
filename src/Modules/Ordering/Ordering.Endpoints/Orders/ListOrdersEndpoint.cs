using FluentValidation;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Ordering.Application.Orders.Queries.ListOrders;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Endpoints.Orders.Requests;
using MediatR;

namespace Invoria.Ordering.Endpoints.Orders;

public class ListOrdersEndpoint : EndpointBase<ListOrdersRequest, PagingDto<OrderDto>>
{
    private readonly IMediator _mediator;

    public ListOrdersEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("");
        AllowAnonymous();

        Group<OrderRoutingGroup>();
    }

    public override async Task HandleAsync(ListOrdersRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var query = new ListOrdersQuery
        {
            Skip = req.Skip,
            Length = req.Length,
            OrderNumber = req.OrderNumber,
            IncludeOrderItems = req.IncludeOrderItems
        };

        var result = await _mediator.Send(query, ct);

        await SendResultAsync(result, ct);
    }
}
