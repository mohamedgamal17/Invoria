using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Financial.Contracts.Payables.Dtos;
using Invoria.Financial.Domain.Payables;

namespace Invoria.Financial.Application.Payables.Factories;

public sealed class PayableResponseFactory : ResponseFactory<Payable, PayableDto>, IPayableResponseFactory
{
    public override Task<PayableDto> PrepareDto(Payable view)
    {
        var settlementDtos = view.Settlements
            .OrderBy(s => s.SettledAt)
            .Select(s => new PayableSettlementDto
            {
                Id = s.Id,
                PayableId = s.PayableId,
                Amount = s.Amount,
                SettledAt = s.SettledAt
            })
            .ToList();

        var dto = new PayableDto
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
