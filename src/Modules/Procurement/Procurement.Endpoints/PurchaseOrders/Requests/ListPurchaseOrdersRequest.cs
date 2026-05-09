using FastEndpoints;
using FluentValidation;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.Procurement.Contracts.PurchaseOrders;
using Invoria.Procurement.Domain.PurchaseOrders;

namespace Invoria.Procurement.Endpoints.PurchaseOrders.Requests;

public sealed class ListPurchaseOrdersRequest : PagingParams
{
    [QueryParam]
    public string? Number { get; set; }

    [QueryParam]
    public PurchaseState? Status { get; set; }

    [QueryParam]
    public bool IncludePurchaseItems { get; set; }

    [QueryParam]
    public bool IncludeSupplier { get; set; }
}

public sealed class ListPurchaseOrdersRequestValidator : AbstractValidator<ListPurchaseOrdersRequest>
{
    public ListPurchaseOrdersRequestValidator()
    {
        Include(new PagingParamasValidator<ListPurchaseOrdersRequest>());

        When(x => !string.IsNullOrWhiteSpace(x.Number), () =>
        {
            RuleFor(x => x.Number!)
                .MaximumLength(PurchaseOrderTableConsts.PurchaseNumberMaxLength);
        });
    }
}
