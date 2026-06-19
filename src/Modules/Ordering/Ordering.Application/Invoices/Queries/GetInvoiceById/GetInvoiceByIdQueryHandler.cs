using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Application.Invoices.Factories;
using Invoria.Ordering.Contracts.Invoices.Dtos;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Invoices;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Ordering.Application.Invoices.Queries.GetInvoiceById;

public class GetInvoiceByIdQueryHandler : IApplicatonRequestHandler<GetInvoiceByIdQuery, InvoiceDto>
{
    private readonly IOrderingRepository<Invoice> _invoiceRepository;
    private readonly IInvoiceResponseFactory _invoiceResponseFactory;

    public GetInvoiceByIdQueryHandler(
        IOrderingRepository<Invoice> invoiceRepository,
        IInvoiceResponseFactory invoiceResponseFactory)
    {
        _invoiceRepository = invoiceRepository;
        _invoiceResponseFactory = invoiceResponseFactory;
    }

    public async Task<Result<InvoiceDto>> Handle(
        GetInvoiceByIdQuery request,
        CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository
            .AsQuerable()
            .Include(i => i.Items)
            .SingleOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (invoice == null)
        {
            return Result.Failure<InvoiceDto>(
                new NotFoundException($"Invoice with ID {request.Id} not found"));
        }

        var dto = await _invoiceResponseFactory.PrepareDto(invoice);

        return dto;
    }
}
