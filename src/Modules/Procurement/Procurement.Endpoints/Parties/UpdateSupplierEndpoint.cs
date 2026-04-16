using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.Results;
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
    }

    public override async Task HandleAsync(UpdateSupplierRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var command = new UpdateSupplierCommand(req.Id, req.SupplierCode, req.Name, req.ContactEmail, req.Phone);
        var result = await _mediator.Send(command, ct);

        await SendResultAsync(result, ct);
    }
}

