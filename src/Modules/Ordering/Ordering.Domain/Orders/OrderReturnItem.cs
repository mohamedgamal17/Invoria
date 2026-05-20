using Ardalis.GuardClauses;

namespace Invoria.Ordering.Domain.Orders;

public class OrderReturnItem
{
    public string OrderItemId { get; private set; } = null!;
    public int Quantity { get; private set; }

    private OrderReturnItem()
    {
    }

    public OrderReturnItem(string orderItemId, int quantity)
    {
        Guard.Against.NullOrWhiteSpace(orderItemId);
        Guard.Against.NegativeOrZero(quantity);

        OrderItemId = orderItemId;
        Quantity = quantity;
    }
}
