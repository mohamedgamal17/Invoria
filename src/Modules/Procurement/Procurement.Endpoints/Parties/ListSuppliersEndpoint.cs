using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Microsoft.AspNetCore.Http;
using Invoria.Procurement.Application.Parties.Queries.ListSuppliers;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Endpoints.Parties.Requests;
using MediatR;

namespace Invoria.Procurement.Endpoints.Parties;

public sealed class ListSuppliersEndpoint : EndpointBase<ListSuppliersRequest, PagingDto<SupplierDto>>
{
    private readonly IMediator _mediator;

    public ListSuppliersEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("");
        AllowAnonymous();
        Group<SupplierRoutingGroup>();

        Summary(s =>
        {
            s.Summary = "List suppliers";
            s.Description = "Returns a paged list of suppliers with optional name or code filters.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns paged supplier data.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(ListSuppliersRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var query = new ListSupplierQuery
        {
            Skip = req.Skip,
            Length = req.Length,
            Name = req.Name,
            Code = req.Code
        };

        var result = await _mediator.Send(query, ct);
        await SendResultAsync(result, ct);
    }
}
