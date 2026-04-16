using FluentValidation;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.Results;
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
