using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Ordering.Application.Invoices.Queries.ListInvoices;
using Invoria.Ordering.Contracts.Invoices.Dtos;
using Invoria.Ordering.Endpoints.Invoices.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Invoria.Ordering.Endpoints.Invoices;

public class ListInvoicesEndpoint : EndpointBase<ListInvoicesRequest, PagingDto<InvoiceDto>>
{
    private readonly IMediator _mediator;

    public ListInvoicesEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("");
        AllowAnonymous();

        Group<InvoiceRoutingGroup>();

        Summary(s =>
        {
            s.Summary = "List invoices";
            s.Description =
                "Returns a paged list of invoices with optional filters by customer id and order id.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns paged invoice data.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status500InternalServerError] =
                InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(ListInvoicesRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var query = new ListInvoicesQuery
        {
            Skip = req.Skip,
            Length = req.Length,
            CustomerId = req.CustomerId,
            OrderId = req.OrderId
        };

        var result = await _mediator.Send(query, ct);

        await SendResultAsync(result, ct);
    }
}
