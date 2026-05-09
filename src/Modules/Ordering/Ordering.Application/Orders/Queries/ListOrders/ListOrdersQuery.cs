using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Contracts.Orders;

namespace Invoria.Ordering.Application.Orders.Queries.ListOrders;

public class ListOrdersQuery : PagingParams, IQuery<PagingDto<OrderDto>>
{
    /// <summary>
    /// When set (non-whitespace), only orders whose order number starts with this value (prefix / autocomplete-style) are returned.
    /// </summary>
    public string? OrderNumber { get; set; }

    /// <summary>
    /// When set (non-whitespace), only orders matching this customer id are returned.
    /// </summary>
    public string? CustomerId { get; set; }

    /// <summary>
    /// When true, line items and catalog product data are loaded. Default is false (header-level list only).
    /// </summary>
    public bool IncludeOrderItems { get; set; }

    /// <summary>
    /// When set, only orders matching this payment type are returned.
    /// </summary>
    public OrderPaymentType? PaymentType { get; set; }

    /// <summary>
    /// When set, only orders matching this payment status are returned.
    /// </summary>
    public OrderPaymentStatus? PaymentStatus { get; set; }

    /// <summary>
    /// When set, only orders in this lifecycle status are returned.
    /// </summary>
    public OrderStatus? Status { get; set; }

    /// <summary>
    /// When set, only orders matching this fulfillment status are returned.
    /// </summary>
    public FullfillmentStatus? FullfillmentStatus { get; set; }
}
