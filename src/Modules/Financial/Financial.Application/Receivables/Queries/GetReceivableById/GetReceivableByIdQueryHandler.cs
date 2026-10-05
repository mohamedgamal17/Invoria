using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Financial.Application.Receivables.Factories;
using Invoria.Financial.Contracts.Receivables.Dtos;
using Invoria.Financial.Domain.Receivables;
using Invoria.Financial.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Financial.Application.Receivables.Queries.GetReceivableById;

public class GetReceivableByIdQueryHandler : IApplicatonRequestHandler<GetReceivableByIdQuery, ReceivableDto>
{
    private readonly IFinancialRepository<Receivable> _receivableRepository;
    private readonly IReceivableResponseFactory _receivableResponseFactory;

    public GetReceivableByIdQueryHandler(
        IFinancialRepository<Receivable> receivableRepository,
        IReceivableResponseFactory receivableResponseFactory)
    {
        _receivableRepository = receivableRepository;
        _receivableResponseFactory = receivableResponseFactory;
    }

    public async Task<Result<ReceivableDto>> Handle(
        GetReceivableByIdQuery request,
        CancellationToken cancellationToken)
    {
        var queryable = _receivableRepository.AsQuerable();

        var receivable = await queryable
            .Include(r => r.Settlements)
            .SingleOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (receivable == null)
        {
            return Result.Failure<ReceivableDto>(
                new NotFoundException($"Receivable with ID {request.Id} not found"));
        }

        var dto = await _receivableResponseFactory.PrepareDto(receivable);

        return dto;
    }
}
