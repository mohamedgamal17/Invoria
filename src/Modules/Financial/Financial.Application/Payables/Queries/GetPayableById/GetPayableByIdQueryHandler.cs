using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Financial.Application.Payables.Factories;
using Invoria.Financial.Contracts.Payables.Dtos;
using Invoria.Financial.Domain.Payables;
using Invoria.Financial.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Financial.Application.Payables.Queries.GetPayableById;

public class GetPayableByIdQueryHandler : IApplicatonRequestHandler<GetPayableByIdQuery, PayableDto>
{
    private readonly IFinancialRepository<Payable> _payableRepository;
    private readonly IPayableResponseFactory _payableResponseFactory;

    public GetPayableByIdQueryHandler(
        IFinancialRepository<Payable> payableRepository,
        IPayableResponseFactory payableResponseFactory)
    {
        _payableRepository = payableRepository;
        _payableResponseFactory = payableResponseFactory;
    }

    public async Task<Result<PayableDto>> Handle(
        GetPayableByIdQuery request,
        CancellationToken cancellationToken)
    {
        var queryable = _payableRepository.AsQuerable();

        var payable = await queryable
            .Include(p => p.Settlements)
            .SingleOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (payable == null)
        {
            return Result.Failure<PayableDto>(
                new NotFoundException($"Payable with ID {request.Id} not found"));
        }

        var dto = await _payableResponseFactory.PrepareDto(payable);

        return dto;
    }
}
