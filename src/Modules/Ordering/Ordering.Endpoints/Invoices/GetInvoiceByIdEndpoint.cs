using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Ordering.Application.Invoices.Queries.GetInvoiceById;
using Invoria.Ordering.Contracts.Invoices.Dtos;
using Invoria.Ordering.Endpoints.Invoices.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Invoria.Ordering.Endpoints.Invoices;

public class GetInvoiceByIdEndpoint : EndpointBase<GetInvoiceByIdRequest, InvoiceDto>
{
    private readonly IMediator _mediator;

    public GetInvoiceByIdEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("{id}");
        AllowAnonymous();

        Group<InvoiceRoutingGroup>();

        Summary(s =>
        {
            s.Summary = "Get invoice by id";
            s.Description = "Returns a single invoice by identifier, including line items.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns the invoice DTO.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status404NotFound] = InvoriaOpenApiResponseDescriptions.NotFound404;
            s.Responses[StatusCodes.Status500InternalServerError] =
                InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(GetInvoiceByIdRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var query = new GetInvoiceByIdQuery
        {
            Id = req.Id
        };

        var result = await _mediator.Send(query, ct);

        await SendResultAsync(result, ct);
    }
}
