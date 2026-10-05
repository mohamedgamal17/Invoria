namespace Invoria.Financial.Contracts.Receivables.Dtos;

public class ReceivableSettlementDto
{
    public string Id { get; set; } = string.Empty;
    public string ReceivableId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTimeOffset SettledAt { get; set; }
}
