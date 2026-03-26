using FluentValidation;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Ordering.Application.Orders.Commands.RefuseOrder;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Endpoints.Orders.Requests;
using MediatR;

namespace Invoria.Ordering.Endpoints.Orders;

public class RefuseOrderEndpoint : EndpointBase<RefuseOrderRequest, OrderDto>
{
    private readonly IMediator _mediator;

    public RefuseOrderEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("{id}/refuse");
        AllowAnonymous();

        Group<OrderRoutingGroup>();
    }

    public override async Task HandleAsync(RefuseOrderRequest req, CancellationToken ct)
    {
        var validator = Resolve<IValidator<RefuseOrderRequest>>();

        var validationResult = validator.Validate(req);

        if (!validationResult.IsValid)
        {
            foreach (var failure in validationResult.Errors)
            {
                AddError(failure.PropertyName, failure.ErrorMessage);
            }

            ThrowIfAnyErrors();
        }

        var command = new RefuseOrderCommand(req.Id);

        var result = await _mediator.Send(command, ct);

        await SendResultAsync(result, ct);
    }
}

