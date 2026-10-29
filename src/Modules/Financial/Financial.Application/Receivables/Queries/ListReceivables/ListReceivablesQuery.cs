using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.Financial.Contracts.Receivables.Dtos;

namespace Invoria.Financial.Application.Receivables.Queries.ListReceivables;

public class ListReceivablesQuery : PagingParams, IQuery<PagingDto<ReceivableDto>>
{
    public string? PartyId { get; set; }

    public string? SourceId { get; set; }

    public bool? IsPaid { get; set; }
}
