using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Financial.Contracts.Receivables.Dtos;

namespace Invoria.Financial.Application.Receivables.Queries.GetReceivableById;

public class GetReceivableByIdQuery : IQuery<ReceivableDto>
{
    public string Id { get; set; } = string.Empty;
}
