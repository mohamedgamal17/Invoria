using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Application.Invoices.Factories;
using Invoria.Ordering.Application.Invoices.Services;
using Invoria.Ordering.Contracts.Invoices.Dtos;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Invoices;
using Invoria.Ordering.Domain.Invoices.Services;
using Invoria.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Ordering.Application.Invoices.Commands.CreateInvoice;

public class CreateInvoiceCommandHandler : IApplicatonRequestHandler<CreateInvoiceCommand, InvoiceDto>
{
    private readonly IOrderingRepository<Order> _orderRepository;
    private readonly IOrderingRepository<Invoice> _invoiceRepository;
    private readonly IInvoiceDomainService _invoiceDomainService;
    private readonly IInvoiceResponseFactory _invoiceResponseFactory;
    private readonly IInvoiceNumberGenerator _invoiceNumberGenerator;

    public CreateInvoiceCommandHandler(
        IOrderingRepository<Order> orderRepository,
        IOrderingRepository<Invoice> invoiceRepository,
        IInvoiceDomainService invoiceDomainService,
        IInvoiceResponseFactory invoiceResponseFactory,
        IInvoiceNumberGenerator invoiceNumberGenerator)
    {
        _orderRepository = orderRepository;
        _invoiceRepository = invoiceRepository;
        _invoiceDomainService = invoiceDomainService;
        _invoiceResponseFactory = invoiceResponseFactory;
        _invoiceNumberGenerator = invoiceNumberGenerator;
    }

    public async Task<Result<InvoiceDto>> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository
            .AsQuerable()
            .Include(o => o.Items)
            .Include(o => o.ReturnItems)
            .SingleAsync(o => o.Id == request.OrderId, cancellationToken);

        var invoiceNumber = await _invoiceNumberGenerator.GenerateAsync(cancellationToken);

        var invoice = _invoiceDomainService.CreateFromOrder(order, invoiceNumber);

        await _invoiceRepository.Add(invoice, cancellationToken);

        var dto = await _invoiceResponseFactory.PrepareDto(invoice);

        return dto;
    }
}
