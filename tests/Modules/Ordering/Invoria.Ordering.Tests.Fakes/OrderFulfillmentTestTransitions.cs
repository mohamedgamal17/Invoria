using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Ordering.Tests.Fakes;

public static class OrderFulfillmentTestTransitions
{
    public static async Task DispatchAndShipAsync(
        IOrderingRepository<Order> repository,
        string orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await repository
            .AsQuerable()
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .SingleOrDefaultAsync(o => o.Id == orderId, cancellationToken)
            ?? throw new InvalidOperationException($"Order with ID {orderId} not found.");

        order.MarkDispatched();
        order.MarkShipped();
        await repository.Update(order, cancellationToken);
    }
}
