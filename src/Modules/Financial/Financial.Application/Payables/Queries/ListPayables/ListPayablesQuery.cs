using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.Financial.Contracts.Payables.Dtos;

namespace Invoria.Financial.Application.Payables.Queries.ListPayables;

public class ListPayablesQuery : PagingParams, IQuery<PagingDto<PayableDto>>
{
    public string? PartyId { get; set; }

    public string? SourceId { get; set; }

    public bool? IsPaid { get; set; }
}
