using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Financial.Application.Receivables.Factories;
using Invoria.Financial.Contracts.Receivables.Dtos;
using Invoria.Financial.Domain.Receivables;
using Invoria.Financial.Domain.Repositories;

namespace Invoria.Financial.Application.Receivables.Commands.CreateReceivable;

public sealed class CreateReceivableCommandHandler : IApplicatonRequestHandler<CreateReceivableCommand, ReceivableDto>
{
    private readonly IFinancialRepository<Receivable> _receivableRepository;
    private readonly IReceivableResponseFactory _receivableResponseFactory;

    public CreateReceivableCommandHandler(
        IFinancialRepository<Receivable> receivableRepository,
        IReceivableResponseFactory receivableResponseFactory)
    {
        _receivableRepository = receivableRepository;
        _receivableResponseFactory = receivableResponseFactory;
    }

    public async Task<Result<ReceivableDto>> Handle(CreateReceivableCommand request, CancellationToken cancellationToken)
    {
        var receivable = Receivable.Create(
            partyId: request.PartyId,
            sourceId: request.SourceId,
            amount: request.Amount);

        await _receivableRepository.Add(receivable, cancellationToken);

        var dto = await _receivableResponseFactory.PrepareDto(receivable);

        return dto;
    }
}
