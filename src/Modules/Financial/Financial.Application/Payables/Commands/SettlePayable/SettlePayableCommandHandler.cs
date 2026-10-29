using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Financial.Application.Payables.Factories;
using Invoria.Financial.Contracts.Payables.Dtos;
using Invoria.Financial.Contracts.Receivables.Enums;
using Invoria.Financial.Domain.Payables;
using Invoria.Financial.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Financial.Application.Payables.Commands.SettlePayable;

public sealed class SettlePayableCommandHandler : IApplicatonRequestHandler<SettlePayableCommand, PayableDto>
{
    private readonly IFinancialRepository<Payable> _payableRepository;
    private readonly IPayableResponseFactory _payableResponseFactory;

    public SettlePayableCommandHandler(
        IFinancialRepository<Payable> payableRepository,
        IPayableResponseFactory payableResponseFactory)
    {
        _payableRepository = payableRepository;
        _payableResponseFactory = payableResponseFactory;
    }

    public async Task<Result<PayableDto>> Handle(SettlePayableCommand request, CancellationToken cancellationToken)
    {
        var query = _payableRepository.AsQuerable();

        var queryWithSettlements = query.Include(p => p.Settlements);

        var payable = await queryWithSettlements.SingleOrDefaultAsync(p => p.Id == request.PayableId, cancellationToken);

        if (payable == null)
        {
            return Result.Failure<PayableDto>(new NotFoundException($"Payable with ID {request.PayableId} not found"));
        }

        if (payable.Status == FinancialObligationStatus.Paid)
        {
            return Result.Failure<PayableDto>(new BusinessLogicException(
                $"Payable with ID {request.PayableId} is already fully settled."));
        }

        if (request.Amount > payable.OutstandingAmount)
        {
            return Result.Failure<PayableDto>(new BusinessLogicException(
                $"Settlement amount {request.Amount} exceeds the outstanding balance {payable.OutstandingAmount} on payable {request.PayableId}."));
        }

        payable.Settle(request.Amount);

        await _payableRepository.Update(payable, cancellationToken);

        var dto = await _payableResponseFactory.PrepareDto(payable);

        return Result.Success(dto);
    }
}
