namespace Invoria.Inventory.Contracts.Returns.Dtos;

public class ReturnLineDto
{
    public string Id { get; set; } = string.Empty;

    public string ReturnId { get; set; } = string.Empty;

    public string OrderItemId { get; set; } = string.Empty;

    public string ProductId { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public ReturnProductDto? Product { get; set; }
}
