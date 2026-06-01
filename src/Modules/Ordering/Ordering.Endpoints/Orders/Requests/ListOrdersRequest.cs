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

    /// <summary>When true, line items and catalog product data are included. Default is false.</summary>
    [QueryParam]
    public bool IncludeOrderItems { get; set; }

    /// <summary>When true, return lines and pricing totals are included. Default is false.</summary>
    [QueryParam]
    public bool IncludeReturnItems { get; set; }

    [QueryParam]
    public OrderPaymentType? PaymentType { get; set; }

    [QueryParam]
    public OrderPaymentStatus? PaymentStatus { get; set; }

    [QueryParam]
    public OrderStatus? Status { get; set; }
}

public class ListOrdersRequestValidator : AbstractValidator<ListOrdersRequest>
{
    public ListOrdersRequestValidator()
    {
        Include(new PagingParamasValidator<ListOrdersRequest>());
    }
}
