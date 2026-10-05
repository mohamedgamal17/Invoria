using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Financial.Application.Receivables.Factories;
using Invoria.Financial.Contracts.Receivables.Dtos;
using Invoria.Financial.Contracts.Receivables.Enums;
using Invoria.Financial.Domain.Receivables;
using Invoria.Financial.Domain.Repositories;

namespace Invoria.Financial.Application.Receivables.Queries.ListReceivables;

public class ListReceivablesQueryHandler : IApplicatonRequestHandler<ListReceivablesQuery, PagingDto<ReceivableDto>>
{
    private readonly IFinancialRepository<Receivable> _receivableRepository;
    private readonly IReceivableResponseFactory _receivableResponseFactory;

    public ListReceivablesQueryHandler(
        IFinancialRepository<Receivable> receivableRepository,
        IReceivableResponseFactory receivableResponseFactory)
    {
        _receivableRepository = receivableRepository;
        _receivableResponseFactory = receivableResponseFactory;
    }

    public async Task<Result<PagingDto<ReceivableDto>>> Handle(
        ListReceivablesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _receivableRepository.AsQuerable();

        var partyIdTerm = request.PartyId?.Trim();

        if (!string.IsNullOrEmpty(partyIdTerm))
        {
            query = query.Where(r => r.PartyId == partyIdTerm);
        }

        var sourceIdTerm = request.SourceId?.Trim();

        if (!string.IsNullOrEmpty(sourceIdTerm))
        {
            query = query.Where(r => r.SourceId == sourceIdTerm);
        }

        if (request.IsPaid.HasValue)
        {
            var isPaid = request.IsPaid.Value;

            query = isPaid
                ? query.Where(r => r.Status == FinancialObligationStatus.Paid)
                : query.Where(r => r.Status != FinancialObligationStatus.Paid);
        }

        var orderedQuery = query.OrderByDescending(r => r.CreatedAt);

        var paged = await orderedQuery.ToPaged(request.Skip, request.Length);
        var response = await _receivableResponseFactory.PreparePagingDto(paged);

        return response;
    }
}
