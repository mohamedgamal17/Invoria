using FluentValidation;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Microsoft.AspNetCore.Http;
using Invoria.Procurement.Application.Parties.Queries.GetSupplierById;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Endpoints.Parties.Requests;
using MediatR;

namespace Invoria.Procurement.Endpoints.Parties;

public sealed class GetSupplierByIdEndpoint : EndpointBase<GetSupplierByIdRequest, SupplierDto>
{
    private readonly IMediator _mediator;

    public GetSupplierByIdEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("{id}");
        AllowAnonymous();
        Group<SupplierRoutingGroup>();

        Summary(s =>
        {
            s.Summary = "Get supplier by id";
            s.Description = "Returns a supplier by identifier.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns the supplier DTO.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status404NotFound] = InvoriaOpenApiResponseDescriptions.NotFound404;
            s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(GetSupplierByIdRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var query = new GetSupplierByIdQuery
        {
            Id = req.Id
        };

        var result = await _mediator.Send(query, ct);
        await SendResultAsync(result, ct);
    }
}
