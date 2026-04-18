using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Microsoft.AspNetCore.Http;
using Invoria.Procurement.Application.Parties.Commands.UpdateSupplier;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Endpoints.Parties.Requests;
using MediatR;

namespace Invoria.Procurement.Endpoints.Parties;

public sealed class UpdateSupplierEndpoint : EndpointBase<UpdateSupplierRequest, SupplierDto>
{
    private readonly IMediator _mediator;

    public UpdateSupplierEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("{id}");
        AllowAnonymous();
        Group<SupplierRoutingGroup>();

        Summary(s =>
        {
            s.Summary = "Update supplier";
            s.Description = "Updates supplier details by identifier.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns the updated supplier.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status404NotFound] = InvoriaOpenApiResponseDescriptions.NotFound404;
            s.Responses[StatusCodes.Status409Conflict] = InvoriaOpenApiResponseDescriptions.Conflict409;
            s.Responses[StatusCodes.Status422UnprocessableEntity] = InvoriaOpenApiResponseDescriptions.UnprocessableEntity422;
            s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(UpdateSupplierRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var command = new UpdateSupplierCommand(req.Id, req.SupplierCode, req.Name, req.ContactEmail, req.Phone);
        var result = await _mediator.Send(command, ct);

        await SendResultAsync(result, ct);
    }
}

