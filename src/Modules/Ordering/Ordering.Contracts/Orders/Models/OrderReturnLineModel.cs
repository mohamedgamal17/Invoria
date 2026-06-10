namespace Invoria.Ordering.Contracts.Orders.Models;

public class OrderReturnLineModel
{
    public string OrderItemId { get; set; } = null!;

    public string ProductId { get; set; } = null!;

    public int Quantity { get; set; }
}
