using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Tests.Fakes;

/// <summary>
/// Builds valid in-memory <see cref="Order"/> graphs for tests. Orders are independent; list order is not guaranteed after DB queries unless sorted by Id (see list handler).
/// </summary>
public static class OrderTestData
{
    private const string OrderNumberPrefix = "TEST-";

    /// <summary>
    /// Creates <paramref name="count"/> orders sharing one customer when <paramref name="customerId"/> is null.
    /// Each order has at least one line item with random product id, quantity (1–5), and price (1–999.99).
    /// </summary>
    public static List<Order> CreateRandomOrders(int count, string? customerId = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        if (count == 0)
        {
            return new List<Order>();
        }

        var sharedCustomerId = customerId ?? Guid.NewGuid().ToString();
        var random = Random.Shared;
        var orders = new List<Order>(count);

        for (var i = 0; i < count; i++)
        {
            var orderNumber = $"{OrderNumberPrefix}{i}-{random.Next(1000, 9999)}-{Guid.NewGuid().ToString("N")[..8]}";
            var order = new Order(orderNumber, sharedCustomerId);

            var lineCount = random.Next(1, 4);
            var items = new List<OrderItem>(lineCount);
            for (var j = 0; j < lineCount; j++)
            {
                items.Add(new OrderItem(
                    Guid.NewGuid().ToString(),
                    random.Next(1, 6),
                    Math.Round((decimal)random.Next(100, 100000) / 100m, 2)));
            }

            order.UpdateItems(items);
            orders.Add(order);
        }

        return orders;
    }

    /// <summary>
    /// Persists <paramref name="count"/> random orders and returns them with ids populated.
    /// </summary>
    public static async Task<List<Order>> PersistRandomOrdersAsync(
        IOrderingRepository<Order> repository,
        int count,
        CancellationToken cancellationToken = default)
    {
        var orders = CreateRandomOrders(count);
        foreach (var order in orders)
        {
            await repository.Add(order, cancellationToken);
        }

        return orders;
    }
}
