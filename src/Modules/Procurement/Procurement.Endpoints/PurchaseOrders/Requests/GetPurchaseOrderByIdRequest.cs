using FastEndpoints;
using FluentValidation;
using Invoria.Procurement.Domain.PurchaseOrders;

namespace Invoria.Procurement.Endpoints.PurchaseOrders.Requests;

public sealed class GetPurchaseOrderByIdRequest
{
    [RouteParam]
    public string Id { get; set; } = string.Empty;
}

public sealed class GetPurchaseOrderByIdRequestValidator : AbstractValidator<GetPurchaseOrderByIdRequest>
{
    public GetPurchaseOrderByIdRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .MaximumLength(PurchaseOrderTableConsts.IdMaxLength);
    }
}
