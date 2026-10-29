using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Financial.Contracts.Receivables.Dtos;
using Invoria.Financial.Domain.Receivables;

namespace Invoria.Financial.Application.Receivables.Factories;

public interface IReceivableResponseFactory : IResponseFactory<Receivable, ReceivableDto>
{
}
