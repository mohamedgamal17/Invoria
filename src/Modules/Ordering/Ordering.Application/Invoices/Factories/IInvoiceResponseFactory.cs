using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Ordering.Contracts.Invoices.Dtos;
using Invoria.Ordering.Domain.Invoices;

namespace Invoria.Ordering.Application.Invoices.Factories;

public interface IInvoiceResponseFactory : IResponseFactory<Invoice, InvoiceDto>
{
}
