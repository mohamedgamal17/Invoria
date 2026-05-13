namespace Invoria.Ordering.Contracts.Models;

/// <summary>
/// Order line item payload for integration events and cross-module messaging.
/// </summary>
public class OrderLineModel
{
    public required string Id { get; set; }

    public required string ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; }
}
