namespace Invoria.Financial.Contracts.Receivables.Events;

public class CreateOrderReceivableIntegrationEvent
{
    public string OrderId { get; set; } = null!;

    public string CustomerId { get; set; } = null!;

    public decimal Amount { get; set; }
}
