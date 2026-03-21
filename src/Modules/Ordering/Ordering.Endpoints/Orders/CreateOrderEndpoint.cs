using FluentValidation;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Ordering.Application.Orders.Commands.CreateOrder;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Endpoints.Orders.Requests;
using MediatR;

namespace Invoria.Ordering.Endpoints.Orders;

public class CreateOrderEndpoint : EndpointBase<CreateOrderRequest, OrderDto>
{
    private readonly IMediator _mediator;

    public CreateOrderEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("");
        AllowAnonymous();

        Group<OrderRoutingGroup>();
    }

    public override async Task HandleAsync(CreateOrderRequest req, CancellationToken ct)
    {
        var validator = Resolve<IValidator<CreateOrderRequest>>();

        var validationResult = validator.Validate(req);

        if (!validationResult.IsValid)
        {
            foreach (var failure in validationResult.Errors)
            {
                AddError(failure.PropertyName, failure.ErrorMessage);
            }

            ThrowIfAnyErrors();
        }

        var itemCommands = req.Items
            .Select(i => new CreateOrderItemCommand(i.ProductId, i.Quantity, i.Price))
            .ToList();

        var command = new CreateOrderCommand(req.CustomerId, itemCommands);

        var result = await _mediator.Send(command, ct);

        await SendResultAsync(result, ct);
    }
}
