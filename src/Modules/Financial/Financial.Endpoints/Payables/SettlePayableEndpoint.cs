using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Financial.Application.Payables.Commands.SettlePayable;
using Invoria.Financial.Contracts.Payables.Dtos;
using Invoria.Financial.Endpoints.Payables.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Invoria.Financial.Endpoints.Payables;

public class SettlePayableEndpoint : EndpointBase<SettlePayableRequest, PayableDto>
{
    private readonly IMediator _mediator;

    public SettlePayableEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("{id}/settle");
        AllowAnonymous();

        Group<PayableRoutingGroup>();

        Summary(s =>
        {
            s.Summary = "Settle a payable";
            s.Description =
                "Settles the given amount against the payable's outstanding balance. "
                + "The amount is validated against the outstanding amount before the settlement is applied.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns the updated payable DTO.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status404NotFound] = InvoriaOpenApiResponseDescriptions.NotFound404;
            s.Responses[StatusCodes.Status500InternalServerError] =
                InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(SettlePayableRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var command = new SettlePayableCommand(req.Id, req.Amount);

        var result = await _mediator.Send(command, ct);

        await SendResultAsync(result, ct);
    }
}
