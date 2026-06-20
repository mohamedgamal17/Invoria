using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;

namespace Invoria.Ordering.Application.Orders.Commands.RecordOrderInvoice;

public sealed class RecordOrderInvoiceCommand : ICommand<Empty>
{
    public RecordOrderInvoiceCommand(string orderId, string invoiceId)
    {
        OrderId = orderId;
        InvoiceId = invoiceId;
    }

    public string OrderId { get; }

    public string InvoiceId { get; }
}
