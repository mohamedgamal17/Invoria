using FastEndpoints;
using FluentValidation;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.Ordering.Contracts.Orders;

namespace Invoria.Ordering.Endpoints.Orders.Requests;

public class ListOrdersRequest : PagingParams
{
    [QueryParam]
    public string? OrderNumber { get; set; }

    [QueryParam]
    public string? CustomerId { get; set; }

    [QueryParam]
    public bool IncludeOrderItems { get; set; }

    [QueryParam]
    public OrderPaymentType? PaymentType { get; set; }

    [QueryParam]
    public OrderPaymentStatus? PaymentStatus { get; set; }
}

public class ListOrdersRequestValidator : AbstractValidator<ListOrdersRequest>
{
    public ListOrdersRequestValidator()
    {
        Include(new PagingParamasValidator<ListOrdersRequest>());
    }
}
