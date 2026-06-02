namespace Invoria.Ordering.Domain.Orders.Events;

public sealed record OrderDispatchedLine(string OrderItemId, string ProductId, int Quantity);
