using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Ordering.Contracts.Invoices.Dtos;

namespace Invoria.Ordering.Application.Invoices.Queries.GetInvoiceById;

public class GetInvoiceByIdQuery : IQuery<InvoiceDto>
{
    public string Id { get; set; } = string.Empty;
}
