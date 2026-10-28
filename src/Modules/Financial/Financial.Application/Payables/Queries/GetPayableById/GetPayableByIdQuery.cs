using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Financial.Contracts.Payables.Dtos;

namespace Invoria.Financial.Application.Payables.Queries.GetPayableById;

public class GetPayableByIdQuery : IQuery<PayableDto>
{
    public string Id { get; set; } = string.Empty;
}
