using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.Ordering.Contracts.Invoices.Dtos;

namespace Invoria.Ordering.Application.Invoices.Queries.ListInvoices;

public class ListInvoicesQuery : PagingParams, IQuery<PagingDto<InvoiceDto>>
{
    /// <summary>
    /// When set (non-whitespace), only invoices for this customer are returned.
    /// </summary>
    public string? CustomerId { get; set; }

    /// <summary>
    /// When set (non-whitespace), only the invoice for this order is returned.
    /// </summary>
    public string? OrderId { get; set; }
}
