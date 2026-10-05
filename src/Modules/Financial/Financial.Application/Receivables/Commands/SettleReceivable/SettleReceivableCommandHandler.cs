using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Financial.Application.Receivables.Factories;
using Invoria.Financial.Contracts.Receivables.Dtos;
using Invoria.Financial.Contracts.Receivables.Enums;
using Invoria.Financial.Domain.Receivables;
using Invoria.Financial.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Financial.Application.Receivables.Commands.SettleReceivable;

public sealed class SettleReceivableCommandHandler : IApplicatonRequestHandler<SettleReceivableCommand, ReceivableDto>
{
    private readonly IFinancialRepository<Receivable> _receivableRepository;
    private readonly IReceivableResponseFactory _receivableResponseFactory;

    public SettleReceivableCommandHandler(
        IFinancialRepository<Receivable> receivableRepository,
        IReceivableResponseFactory receivableResponseFactory)
    {
        _receivableRepository = receivableRepository;
        _receivableResponseFactory = receivableResponseFactory;
    }

    public async Task<Result<ReceivableDto>> Handle(SettleReceivableCommand request, CancellationToken cancellationToken)
    {
        var query = _receivableRepository.AsQuerable();

        var queryWithSettlements = query.Include(r => r.Settlements);

        var receivable = await queryWithSettlements.SingleOrDefaultAsync(r => r.Id == request.ReceivableId, cancellationToken);

        if (receivable == null)
        {
            return Result.Failure<ReceivableDto>(new NotFoundException($"Receivable with ID {request.ReceivableId} not found"));
        }

        if (receivable.Status == FinancialObligationStatus.Paid)
        {
            return Result.Failure<ReceivableDto>(new BusinessLogicException(
                $"Receivable with ID {request.ReceivableId} is already fully settled."));
        }

        if (request.Amount > receivable.OutstandingAmount)
        {
            return Result.Failure<ReceivableDto>(new BusinessLogicException(
                $"Settlement amount {request.Amount} exceeds the outstanding balance {receivable.OutstandingAmount} on receivable {request.ReceivableId}."));
        }

        receivable.Settle(request.Amount);

        await _receivableRepository.Update(receivable, cancellationToken);

        var dto = await _receivableResponseFactory.PrepareDto(receivable);

        return Result.Success(dto);
    }
}
