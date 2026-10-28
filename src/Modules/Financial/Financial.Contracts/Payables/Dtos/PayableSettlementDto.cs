namespace Invoria.Financial.Contracts.Payables.Dtos;

public class PayableSettlementDto
{
    public string Id { get; set; } = string.Empty;
    public string PayableId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTimeOffset SettledAt { get; set; }
}
