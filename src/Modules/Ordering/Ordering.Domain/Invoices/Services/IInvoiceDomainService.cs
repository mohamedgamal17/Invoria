using Invoria.BuildingBlocks.Domain.Services;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Domain.Invoices.Services;

public interface IInvoiceDomainService : IDomainService
{
    Invoice CreateFromOrder(Order order);
}
