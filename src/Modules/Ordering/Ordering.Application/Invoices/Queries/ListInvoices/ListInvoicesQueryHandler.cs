using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Ordering.Application.Invoices.Factories;
using Invoria.Ordering.Contracts.Invoices.Dtos;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Invoices;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Ordering.Application.Invoices.Queries.ListInvoices;

public class ListInvoicesQueryHandler : IApplicatonRequestHandler<ListInvoicesQuery, PagingDto<InvoiceDto>>
{
    private readonly IOrderingRepository<Invoice> _invoiceRepository;
    private readonly IInvoiceResponseFactory _invoiceResponseFactory;

    public ListInvoicesQueryHandler(
        IOrderingRepository<Invoice> invoiceRepository,
        IInvoiceResponseFactory invoiceResponseFactory)
    {
        _invoiceRepository = invoiceRepository;
        _invoiceResponseFactory = invoiceResponseFactory;
    }

    public async Task<Result<PagingDto<InvoiceDto>>> Handle(
        ListInvoicesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _invoiceRepository.AsQuerable();

        var customerIdTerm = request.CustomerId?.Trim();

        if (!string.IsNullOrEmpty(customerIdTerm))
        {
            query = query.Where(i => i.CustomerId == customerIdTerm);
        }

        var orderIdTerm = request.OrderId?.Trim();

        if (!string.IsNullOrEmpty(orderIdTerm))
        {
            query = query.Where(i => i.OrderId == orderIdTerm);
        }

        query = query
            .Include(i => i.Items)
            .OrderByDescending(i => i.Id);

        var paged = await query.ToPaged(request.Skip, request.Length);
        var response = await _invoiceResponseFactory.PreparePagingDto(paged);

        return response;
    }
}
