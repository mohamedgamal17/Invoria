using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Financial.Contracts.Receivables.Dtos;
using Invoria.Financial.Domain.Receivables;

namespace Invoria.Financial.Application.Receivables.Factories;

public sealed class ReceivableResponseFactory : ResponseFactory<Receivable, ReceivableDto>, IReceivableResponseFactory
{
    public override Task<ReceivableDto> PrepareDto(Receivable view)
    {
        var dto = new ReceivableDto
        {
            Id = view.Id,
            PartyId = view.PartyId,
            SourceId = view.SourceId,
            Amount = view.Amount,
            OutstandingAmount = view.OutstandingAmount,
            Status = view.Status
        };

        MapAudited(view, dto);

        return Task.FromResult(dto);
    }
}
