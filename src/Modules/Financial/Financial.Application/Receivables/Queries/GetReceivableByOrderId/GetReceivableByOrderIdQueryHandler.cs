using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Financial.Application.Receivables.Factories;
using Invoria.Financial.Contracts.Receivables.Dtos;
using Invoria.Financial.Domain.Receivables;
using Invoria.Financial.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Financial.Application.Receivables.Queries.GetReceivableByOrderId;

public class GetReceivableByOrderIdQueryHandler : IApplicatonRequestHandler<GetReceivableByOrderIdQuery, ReceivableDto>
{
    private readonly IFinancialRepository<Receivable> _receivableRepository;
    private readonly IReceivableResponseFactory _receivableResponseFactory;

    public GetReceivableByOrderIdQueryHandler(
        IFinancialRepository<Receivable> receivableRepository,
        IReceivableResponseFactory receivableResponseFactory)
    {
        _receivableRepository = receivableRepository;
        _receivableResponseFactory = receivableResponseFactory;
    }

    public async Task<Result<ReceivableDto>> Handle(
        GetReceivableByOrderIdQuery request,
        CancellationToken cancellationToken)
    {
        var queryable = _receivableRepository.AsQuerable();

        var receivable = await queryable
            .Include(r => r.Settlements)
            .SingleOrDefaultAsync(r => r.SourceId == request.OrderId, cancellationToken);

        if (receivable == null)
        {
            return Result.Failure<ReceivableDto>(
                new NotFoundException($"Receivable with OrderId {request.OrderId} not found"));
        }

        var dto = await _receivableResponseFactory.PrepareDto(receivable);

        return dto;
    }
}
