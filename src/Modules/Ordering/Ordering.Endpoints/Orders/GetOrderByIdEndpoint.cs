using FluentValidation;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Ordering.Application.Orders.Queries.GetOrderById;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Endpoints.Orders.Requests;
using MediatR;

namespace Invoria.Ordering.Endpoints.Orders;

public class GetOrderByIdEndpoint : EndpointBase<GetOrderByIdRequest, OrderDto>
{
    private readonly IMediator _mediator;

    public GetOrderByIdEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("{id}");
        AllowAnonymous();

        Group<OrderRoutingGroup>();
    }

    public override async Task HandleAsync(GetOrderByIdRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var query = new GetOrderByIdQuery
        {
            Id = req.Id
        };

        var result = await _mediator.Send(query, ct);

        await SendResultAsync(result, ct);
    }
}
