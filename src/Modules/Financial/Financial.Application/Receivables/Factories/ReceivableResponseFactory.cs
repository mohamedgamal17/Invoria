using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Financial.Contracts.Receivables.Dtos;
using Invoria.Financial.Domain.Receivables;

namespace Invoria.Financial.Application.Receivables.Factories;

public sealed class ReceivableResponseFactory : ResponseFactory<Receivable, ReceivableDto>, IReceivableResponseFactory
{
    public override Task<ReceivableDto> PrepareDto(Receivable view)
    {
        var settlementDtos = view.Settlements
            .OrderBy(s => s.SettledAt)
            .Select(s => new ReceivableSettlementDto
            {
                Id = s.Id,
                ReceivableId = s.ReceivableId,
                Amount = s.Amount,
                SettledAt = s.SettledAt
            })
            .ToList();

        var dto = new ReceivableDto
        {
            Id = view.Id,
            PartyId = view.PartyId,
            SourceId = view.SourceId,
            Amount = view.Amount,
            OutstandingAmount = view.OutstandingAmount,
            Status = view.Status,
            Settlements = settlementDtos
        };

        MapAudited(view, dto);

        return Task.FromResult(dto);
    }
}
