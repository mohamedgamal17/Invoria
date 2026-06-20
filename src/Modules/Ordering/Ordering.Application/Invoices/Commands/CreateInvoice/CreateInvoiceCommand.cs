using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Ordering.Contracts.Invoices.Dtos;

namespace Invoria.Ordering.Application.Invoices.Commands.CreateInvoice;

public class CreateInvoiceCommand : ICommand<InvoiceDto>
{
    public string OrderId { get; set; }

    public CreateInvoiceCommand(string orderId)
    {
        OrderId = orderId;
    }
}
