using FluentValidation;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Microsoft.AspNetCore.Http;
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

        Summary(s =>
        {
            s.Summary = "List orders";
            s.Description = "Returns a paged list of orders with optional filters.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns paged order data.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(ListOrdersRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var query = new ListOrdersQuery
        {
            Skip = req.Skip,
            Length = req.Length,
            OrderNumber = req.OrderNumber,
            CustomerId = req.CustomerId,
            IncludeOrderItems = req.IncludeOrderItems,
            PaymentType = req.PaymentType,
            PaymentStatus = req.PaymentStatus,
            Status = req.Status,
            FullfillmentStatus = req.FullfillmentStatus
        };

        var result = await _mediator.Send(query, ct);

        await SendResultAsync(result, ct);
    }
}
