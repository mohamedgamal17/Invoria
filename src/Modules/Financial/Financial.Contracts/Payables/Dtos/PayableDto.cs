using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.Financial.Contracts.Receivables.Enums;

namespace Invoria.Financial.Contracts.Payables.Dtos;

public sealed class PayableDto : AuditedEntityDto
{
    public string PartyId { get; set; } = default!;
    public string SourceId { get; set; } = default!;
    public decimal Amount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public FinancialObligationStatus Status { get; set; }
    public List<PayableSettlementDto> Settlements { get; set; } = new();
}
