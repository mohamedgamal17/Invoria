using FluentValidation;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Ordering.Application.Orders.Commands.DispatchOrder;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Endpoints.Orders.Requests;
using MediatR;

namespace Invoria.Ordering.Endpoints.Orders;

public class DispatchOrderEndpoint : EndpointBase<DispatchOrderRequest, OrderDto>
{
    private readonly IMediator _mediator;

    public DispatchOrderEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("{id}/dispatch");
        AllowAnonymous();

        Group<OrderRoutingGroup>();
    }

    public override async Task HandleAsync(DispatchOrderRequest req, CancellationToken ct)
    {
        var validator = Resolve<IValidator<DispatchOrderRequest>>();

        var validationResult = validator.Validate(req);

        if (!validationResult.IsValid)
        {
            foreach (var failure in validationResult.Errors)
            {
                AddError(failure.PropertyName, failure.ErrorMessage);
            }

            ThrowIfAnyErrors();
        }

        var command = new DispatchOrderCommand(req.Id);

        var result = await _mediator.Send(command, ct);

        await SendResultAsync(result, ct);
    }
}
