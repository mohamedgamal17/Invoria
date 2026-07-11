using FluentValidation;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Invoria.Inventory.Application.Returns.Commands.ApproveReturn;
using Invoria.Inventory.Contracts.Returns.Dtos;
using Invoria.Inventory.Endpoints.Returns.Requests;

namespace Invoria.Inventory.Endpoints.Returns;

public sealed class ApproveReturnEndpoint
    : EndpointBase<ApproveReturnRequest, ReturnDto>
{
    private readonly IMediator _mediator;

    public ApproveReturnEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("{id}/approve");
        AllowAnonymous();

        Group<ReturnRoutingGroup>();

        Summary(s =>
        {
            s.Summary = "Approve return";
            s.Description = "Approves a pending return. For immediate returns, this triggers stock restoration.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns the approved return DTO.";
            s.Responses[StatusCodes.Status400BadRequest] =
                InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status404NotFound] =
                InvoriaOpenApiResponseDescriptions.NotFound404;
            s.Responses[StatusCodes.Status422UnprocessableEntity] =
                InvoriaOpenApiResponseDescriptions.UnprocessableEntity422;
            s.Responses[StatusCodes.Status500InternalServerError] =
                InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(ApproveReturnRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var command = new ApproveReturnCommand(req.Id);
        var result = await _mediator.Send(command, ct);

        await SendResultAsync(result, ct);
    }
}
