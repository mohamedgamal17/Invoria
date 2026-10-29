using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Financial.Application.Payables.Factories;
using Invoria.Financial.Contracts.Payables.Dtos;
using Invoria.Financial.Contracts.Receivables.Enums;
using Invoria.Financial.Domain.Payables;
using Invoria.Financial.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Financial.Application.Payables.Queries.ListPayables;

public class ListPayablesQueryHandler : IApplicatonRequestHandler<ListPayablesQuery, PagingDto<PayableDto>>
{
    private readonly IFinancialRepository<Payable> _payableRepository;
    private readonly IPayableResponseFactory _payableResponseFactory;

    public ListPayablesQueryHandler(
        IFinancialRepository<Payable> payableRepository,
        IPayableResponseFactory payableResponseFactory)
    {
        _payableRepository = payableRepository;
        _payableResponseFactory = payableResponseFactory;
    }

    public async Task<Result<PagingDto<PayableDto>>> Handle(
        ListPayablesQuery request,
        CancellationToken cancellationToken)
    {
        var queryable = _payableRepository.AsQuerable();

        IQueryable<Payable> query = queryable.Include(p => p.Settlements);

        var partyIdTerm = request.PartyId?.Trim();

        if (!string.IsNullOrEmpty(partyIdTerm))
        {
            query = query.Where(p => p.PartyId == partyIdTerm);
        }

        var sourceIdTerm = request.SourceId?.Trim();

        if (!string.IsNullOrEmpty(sourceIdTerm))
        {
            query = query.Where(p => p.SourceId == sourceIdTerm);
        }

        if (request.IsPaid.HasValue)
        {
            var isPaid = request.IsPaid.Value;

            query = isPaid
                ? query.Where(p => p.Status == FinancialObligationStatus.Paid)
                : query.Where(p => p.Status != FinancialObligationStatus.Paid);
        }

        var orderedQuery = query.OrderByDescending(p => p.CreatedAt);

        var paged = await orderedQuery.ToPaged(request.Skip, request.Length);
        var response = await _payableResponseFactory.PreparePagingDto(paged);

        return response;
    }
}
