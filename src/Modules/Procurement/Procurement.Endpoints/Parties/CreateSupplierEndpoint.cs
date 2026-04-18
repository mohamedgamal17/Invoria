using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Microsoft.AspNetCore.Http;
using Invoria.Procurement.Application.Parties.Commands.CreateSupplier;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Endpoints.Parties.Requests;
using MediatR;

namespace Invoria.Procurement.Endpoints.Parties;

public sealed class CreateSupplierEndpoint : EndpointBase<CreateSupplierRequest, SupplierDto>
{
    private readonly IMediator _mediator;

    public CreateSupplierEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("");
        AllowAnonymous();
        Group<SupplierRoutingGroup>();

        Summary(s =>
        {
            s.Summary = "Create supplier";
            s.Description = "Registers a new procurement supplier.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns the created supplier.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status422UnprocessableEntity] = InvoriaOpenApiResponseDescriptions.UnprocessableEntity422;
            s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(CreateSupplierRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var command = new CreateSupplierCommand(req.SupplierCode, req.Name, req.ContactEmail, req.Phone);
        var result = await _mediator.Send(command, ct);

        await SendResultAsync(result, ct);
    }
}

