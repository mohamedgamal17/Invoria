using FluentValidation;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Inventory.Application.Batches.Queries.GetBatchById;
using Invoria.Inventory.Contracts.Dtos;
using Invoria.Inventory.Endpoints.Batches.Requests;
using MediatR;

namespace Invoria.Inventory.Endpoints.Batches;

public class GetBatchByIdEndpoint : EndpointBase<GetBatchByIdRequest, BatchDto>
{
    private readonly IMediator _mediator;

    public GetBatchByIdEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("{id}");
        AllowAnonymous();

        Group<BatchRoutingGroup>();
    }

    public override async Task HandleAsync(GetBatchByIdRequest req, CancellationToken ct)
    {
        var validator = Resolve<IValidator<GetBatchByIdRequest>>();
        var validationResult = validator.Validate(req);

        if (!validationResult.IsValid)
        {
            foreach (var failure in validationResult.Errors)
            {
                AddError(failure.PropertyName, failure.ErrorMessage);
            }

            ThrowIfAnyErrors();
        }

        var query = new GetBatchByIdQuery
        {
            Id = req.Id
        };

        var result = await _mediator.Send(query, ct);

        await SendResultAsync(result, ct);
    }
}
