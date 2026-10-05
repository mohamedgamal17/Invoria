using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.Financial.Contracts.Receivables.Enums;

namespace Invoria.Financial.Contracts.Receivables.Dtos;

public sealed class ReceivableDto : AuditedEntityDto
{
    public string PartyId { get; set; } = default!;
    public string SourceId { get; set; } = default!;
    public decimal Amount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public FinancialObligationStatus Status { get; set; }
    public List<ReceivableSettlementDto> Settlements { get; set; } = new();
}
