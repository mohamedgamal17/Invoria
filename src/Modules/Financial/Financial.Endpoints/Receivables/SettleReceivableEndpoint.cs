using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Financial.Application.Receivables.Commands.SettleReceivable;
using Invoria.Financial.Contracts.Receivables.Dtos;
using Invoria.Financial.Endpoints.Receivables.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Invoria.Financial.Endpoints.Receivables;

public class SettleReceivableEndpoint : EndpointBase<SettleReceivableRequest, ReceivableDto>
{
    private readonly IMediator _mediator;

    public SettleReceivableEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("{id}/settle");
        AllowAnonymous();

        Group<ReceivableRoutingGroup>();

        Summary(s =>
        {
            s.Summary = "Settle a receivable";
            s.Description =
                "Settles the given amount against the receivable's outstanding balance. "
                + "The amount is validated against the outstanding amount before the settlement is applied.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns the updated receivable DTO.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status404NotFound] = InvoriaOpenApiResponseDescriptions.NotFound404;
            s.Responses[StatusCodes.Status500InternalServerError] =
                InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(SettleReceivableRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var command = new SettleReceivableCommand(req.Id, req.Amount);

        var result = await _mediator.Send(command, ct);

        await SendResultAsync(result, ct);
    }
}
