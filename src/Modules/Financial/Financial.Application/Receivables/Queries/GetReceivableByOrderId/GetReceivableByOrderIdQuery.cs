using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Financial.Contracts.Receivables.Dtos;

namespace Invoria.Financial.Application.Receivables.Queries.GetReceivableByOrderId;

public class GetReceivableByOrderIdQuery : IQuery<ReceivableDto>
{
    public string OrderId { get; set; } = string.Empty;
}
