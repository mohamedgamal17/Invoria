namespace Invoria.Inventory.Contracts.Returns.Models;

public class ReturnLineModel
{
    public string OrderItemId { get; set; } = null!;

    public string ProductId { get; set; } = null!;

    public int Quantity { get; set; }
}
