using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Ordering.Contracts.Invoices.Dtos;
using Invoria.Ordering.Domain.Invoices;

namespace Invoria.Ordering.Application.Invoices.Factories;

public class InvoiceResponseFactory : ResponseFactory<Invoice, InvoiceDto>, IInvoiceResponseFactory
{
    public override Task<InvoiceDto> PrepareDto(Invoice view)
    {
        var dto = new InvoiceDto
        {
            Id = view.Id,
            CustomerId = view.CustomerId,
            OrderId = view.OrderId,
            Subtotal = view.Subtotal,
            TotalPrice = view.TotalPrice,
            Items = view.Items
                .Select(item => new InvoiceItemDto
                {
                    OrderItemId = item.OrderItemId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price
                })
                .ToList()
        };

        MapAudited(view, dto);

        return Task.FromResult(dto);
    }
}
