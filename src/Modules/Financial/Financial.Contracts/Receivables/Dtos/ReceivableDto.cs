using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.CustomerManagement.Contracts.Dtos;
using Invoria.Financial.Contracts.Receivables.Enums;
using Invoria.Ordering.Contracts.Orders.Dtos;

namespace Invoria.Financial.Contracts.Receivables.Dtos;

public sealed class ReceivableDto : AuditedEntityDto
{
    public string PartyId { get; set; } = default!;
    public string SourceId { get; set; } = default!;
    public PublicOrderDto? Source { get; set; }
    public CustomerDto? Party { get; set; }
    public decimal Amount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public FinancialObligationStatus Status { get; set; }
    public List<ReceivableSettlementDto> Settlements { get; set; } = new();
}
