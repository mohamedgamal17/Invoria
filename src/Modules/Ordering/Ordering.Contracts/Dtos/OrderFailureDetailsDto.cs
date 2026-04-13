using Invoria.BuildingBlocks.Domain.Dtos;

namespace Invoria.Ordering.Contracts.Dtos;

public class OrderFailureDetailsDto : AuditedEntityDto
{
    public string ItemId { get; set; } = string.Empty;
    public string? ItemName { get; set; }
    public int QuantityRequested { get; set; }
    public int QuantityAvailable { get; set; }
    public int Shortage { get; set; }
}
