using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Financial.Application.Payables.Factories;
using Invoria.Financial.Contracts.Payables.Dtos;
using Invoria.Financial.Domain.Payables;
using Invoria.Financial.Domain.Repositories;

namespace Invoria.Financial.Application.Payables.Commands.CreatePayable;

public sealed class CreatePayableCommandHandler : IApplicatonRequestHandler<CreatePayableCommand, PayableDto>
{
    private readonly IFinancialRepository<Payable> _payableRepository;
    private readonly IPayableResponseFactory _payableResponseFactory;

    public CreatePayableCommandHandler(
        IFinancialRepository<Payable> payableRepository,
        IPayableResponseFactory payableResponseFactory)
    {
        _payableRepository = payableRepository;
        _payableResponseFactory = payableResponseFactory;
    }

    public async Task<Result<PayableDto>> Handle(CreatePayableCommand request, CancellationToken cancellationToken)
    {
        var payable = Payable.Create(
            partyId: request.PartyId,
            sourceId: request.SourceId,
            amount: request.Amount);

        await _payableRepository.Add(payable, cancellationToken);

        var dto = await _payableResponseFactory.PrepareDto(payable);

        return dto;
    }
}
